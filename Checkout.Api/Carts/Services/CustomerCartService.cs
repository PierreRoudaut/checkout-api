using Checkout.Api.Carts.Models;
using Checkout.Api.Hubs;
using Checkout.Api.Products.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Checkout.Api.Carts.Services
{
    public static class AppEvents
    {
        public const string CartExpired = "CartExpired";
        public const string ProductUpdated = "ProductUpdated";
    }

    public class CustomerCartService
    {
        private readonly IMemoryCache memoryCache;
        private readonly IHubContext<NotifyHub> hubContext;
        private readonly IProductCacheService productCacheService;

        private const string CartCacheKeyFormat = "[cart][{0}]";

        private string CartCacheKey(string cartId) => string.Format(CartCacheKeyFormat, cartId);

        public void StoreCartToCache(string cartId, Cart cart)
        {
            const int expirationMinutes = 1;
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove,
                ExpirationTokens = { new CancellationChangeToken(new CancellationTokenSource(TimeSpan.FromMinutes(expirationMinutes)).Token) },
                PostEvictionCallbacks = { new PostEvictionCallbackRegistration { State = this, EvictionCallback = CacheItemRemoved } }
            };
            if (memoryCache.GetType().Name != "IMemoryCacheProxy") memoryCache.Set(cartId, cart, memoryCacheEntryOptions);
        }

        private void CacheItemRemoved(object key, object value, EvictionReason reason, object state)
        {
            Log.Information("Evicted cache entry " + key + " => " + reason);
            if (reason != EvictionReason.TokenExpired) return;
            var cart = (Cart)value;
            var self = (CustomerCartService)state;
            Log.Information("Cart with id:" + cart.Id + " was removed");

            if (cart.CartItems.Any())
            {
                // notify customer cart expired
                self.hubContext.Clients.All.SendAsync(AppEvents.CartExpired, cart.Id);
                // released retained items
                foreach (var retainedItem in cart.CartItems)
                {
                    self.productCacheService.TryUpdateRetained(-retainedItem.Value.Quantity, retainedItem.Key, out var product, out var error);
                    self.hubContext.Clients?.All?.SendAsync(AppEvents.ProductUpdated, product).Wait();
                }
            }
        }

        public CustomerCartService(IMemoryCache memoryCache, IHubContext<NotifyHub> hubContext, IProductCacheService productCacheService)
        {
            this.memoryCache = memoryCache;
            this.hubContext = hubContext;
            this.productCacheService = productCacheService;
        }

        /// <summary>
        /// Retrieves an existing cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public Cart GetCart(string cartId = null)
        {
            if (cartId != null && memoryCache.TryGetValue(CartCacheKey(cartId), out Cart cart))
            {
                return cart;
            }

            return null;
        }

        /// <summary>
        /// Initialize and empty cart
        /// </summary>
        /// <returns></returns>
        public Cart CreateCart()
        {
            return new Cart
            {
                Id = Guid.NewGuid().ToString("N"),
                CartItems = new Dictionary<int, CartItem>()
            };
        }


        /// <summary>
        /// Clears the items of a given cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public Cart ClearCart(string cartId, out CartOperationError error)
        {
            var cacheKey = CartCacheKey(cartId);
            error = new CartOperationError();
            if (!memoryCache.TryGetValue(cacheKey, out Cart cart))
            {
                error.StatusCode = 404;
                error.Message = "Cart not found";
                return null;
            }

            foreach (var retainedItem in cart.CartItems)
            {
                if (!this.productCacheService.TryUpdateRetained(-retainedItem.Value.Quantity, retainedItem.Key,
                    out var product, out error))
                {
                    Log.Error("Failed to clear cart " + cartId);
                    Log.Error(error.Message);
                    return null;
                }
                this.hubContext.Clients?.All?.SendAsync(AppEvents.ProductUpdated, product).Wait();
            }
            cart.CartItems.Clear();
            StoreCartToCache(cacheKey, cart);
            return cart;
        }

        /// <summary>
        /// Add or update the quantity of a given product into a cart
        /// Also creates a new cart if none exists
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public Cart SetCartItem(string cartId, CartItem item, out CartOperationError error)
        {
            error = new CartOperationError();
            if (item.Quantity <= 0)
            {
                error.StatusCode = 400;
                error.Message = "Invalid quantity";
                return null;
            }

            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                cart = CreateCart();
            }

            var quantity = item.Quantity;
            if (cart.CartItems.TryGetValue(item.ProductId, out var existingItem))
            {
                quantity -= existingItem.Quantity;
            }

            if (!productCacheService.TryUpdateRetained(quantity, item.ProductId, out var product, out error))
            {
                error.StatusCode = 400;
                error.Message = "Failed to add product to cart";
                return null;
            }
            cart.CartItems[item.ProductId] = item;
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            hubContext.Clients.All.SendAsync(AppEvents.ProductUpdated, product).Wait();
            return cart;
        }

        /// <summary>
        /// Removes an item for a given cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="productId"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public Cart RemoveCartItem(string cartId, int productId, out CartOperationError error)
        {
            error = new CartOperationError();
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                error.StatusCode = 404;
                error.Message = "Cart not found";
                return null;
            }

            if (!cart.CartItems.TryGetValue(productId, out var existingItem))
            {
                error.StatusCode = 400;
                error.Message = "Product not added to cart";
                return null;
            }

            if (!productCacheService.TryUpdateRetained(-existingItem.Quantity, productId, out var product, out error))
            {
                return null;
            }

            cart.CartItems.Remove(productId);
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            hubContext.Clients?.All?.SendAsync(AppEvents.ProductUpdated, product);
            return cart;
        }
    }
}