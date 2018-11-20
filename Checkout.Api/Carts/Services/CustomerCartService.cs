using Checkout.Api.Carts.Models;
using Checkout.Api.Hubs;
using Checkout.Api.Products.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Checkout.Api.Products.Services;

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
            if (reason != EvictionReason.TokenExpired) return;
            var cacheCartId = (string)key;
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
                    self.hubContext.Clients.All.SendAsync(AppEvents.ProductUpdated, product).Wait();
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
            if (cartId != null && memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return cart;
            }

            return null;
        }

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
        public Cart ClearCart(string cartId)
        {
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return null;
            }

            foreach (var retainedItem in cart.CartItems)
            {
                this.productCacheService.TryUpdateRetained(-retainedItem.Value.Quantity, retainedItem.Key, out var product, out var error);
                this.hubContext.Clients.All.SendAsync(AppEvents.ProductUpdated, product).Wait();
            }
            cart.CartItems.Clear();
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            return cart;
        }

        /// <summary>
        /// 
        /// Adds or replace (if different quantity) a cart item and 
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
        /// <returns></returns>
        public Cart RemoveCartItem(string cartId, int productId)
        {
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return null;
            }

            if (!cart.CartItems.TryGetValue(productId, out var existingItem))
            {
                return null;
            }

            if (!productCacheService.TryUpdateRetained(-existingItem.Quantity, productId, out var product, out var error))
            {
                return null;
            }

            cart.CartItems.Remove(productId);
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            hubContext.Clients.All.SendAsync(AppEvents.ProductUpdated, product);
            return cart;
        }
    }
}