using Checkout.Api.Carts.Models;
using Checkout.Api.Hubs;
using Checkout.Api.Products.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Checkout.Api.Carts.Services
{
    public class CartOperationError
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class CartService
    {
        private readonly IMemoryCache memoryCache;
        private readonly IProductRepository productRepository;
        private readonly IHubContext<NotifyHub> hubContext;

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
            var self = (CartService)state;
            Log.Information("Cart with id:" + cart.Id + " was removed");
            self.hubContext.Clients.All.SendAsync("CartExpired", cart.Id);
        }

        public CartService(IMemoryCache memoryCache, IProductRepository productRepository, IHubContext<NotifyHub> hubContext)
        {
            this.memoryCache = memoryCache;
            this.productRepository = productRepository;
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Retrieves an existing cart or create a new one
        /// </summary>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public Cart GetOrCreateCart(string cartId = null)
        {
            if (cartId != null && memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return cart;
            }

            cart = new Cart
            {
                Id = Guid.NewGuid().ToString("N"),
                CartItems = new Dictionary<int, CartItem>()
            };

            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            return cart;
        }

        /// <summary>
        /// Clears the items of a given cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <returns></returns>
        public bool ClearCart(string cartId)
        {
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return false;
            }

            cart.CartItems.Clear();
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            return true;
        }

        /// <summary>
        /// Adds or replace (if different quantity) a cart item
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SetCartItem(string cartId, CartItem item, out CartOperationError error)
        {
            error = new CartOperationError();
            if (item.Quantity <= 0)
            {
                error.StatusCode = 400;
                error.Message = "Invalid quantity";
                return false;
            }

            var product = productRepository.Find(item.ProductId);
            if (product == null)
            {
                error.StatusCode = 400;
                error.Message = "Product does not exists";
                return false;
            }

            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                error.StatusCode = 404;
                error.Message = "Cart not found";
                return false;
            }

            cart.CartItems[item.ProductId] = item;
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            hubContext.Clients.All.SendAsync("ReceiveMessage", item);
            return true;
        }

        /// <summary>
        /// Removes an item for a given cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public bool RemoveCartItem(string cartId, int productId)
        {
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return false;
            }

            cart.CartItems.Remove(productId);
            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);

            StoreCartToCache(string.Format(CartCacheKeyFormat, cart.Id), cart);
            return true;
        }
    }
}