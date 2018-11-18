using Checkout.Api.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using Checkout.Api.Carts.Models;

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

        private const string CartCacheKeyFormat = "[cart][{0}]";

        public static MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public CartService(IMemoryCache memoryCache, IProductRepository productRepository)
        {
            this.memoryCache = memoryCache;
            this.productRepository = productRepository;
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

            memoryCache.Set(string.Format(CartCacheKeyFormat, cart.Id), cart, CacheEntryOptions);
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
            memoryCache.Set(cartId, cart, CacheEntryOptions);
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
            memoryCache.Set(cartId, cart, CacheEntryOptions);
            return true;
        }

        /// <summary>
        /// Removes an item for a given cart
        /// </summary>
        /// <param name="cartId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveCartItem(string cartId, int productId)
        {
            if (!memoryCache.TryGetValue(string.Format(CartCacheKeyFormat, cartId), out Cart cart))
            {
                return false;
            }

            cart.CartItems.Remove(productId);
            memoryCache.Set(cartId, cart, CacheEntryOptions);
            return true;
        }
    }
}