using Checkout.Api.Carts.Services;
using Checkout.Api.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace Checkout.Api.Products.Services
{
    public class ProductCacheService : IProductCacheService
    {
        private readonly IProductRepository repository;
        private readonly IMemoryCache memoryCache;

        private const string ProductCacheKeyFormat = "[product][{0}]";

        public string ProductCacheKey(int productId) => string.Format(ProductCacheKeyFormat, productId);


        public ProductCacheService(IProductRepository repository, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Retrieve all products in repository merged with retained data from memory cache
        /// Initialize cache entries if non exist
        /// </summary>
        /// <returns></returns>
        public List<Product> List()
        {
            var products = repository.GetAllProducts();
            foreach (var product in products)
            {
                var productCacheKey = ProductCacheKey(product.Id);
                if (!memoryCache.TryGetValue(productCacheKey, out Product cachedProduct))
                {
                    product.Retained = 0;
                    memoryCache.Set(productCacheKey, product, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.MaxValue,
                        Priority = CacheItemPriority.NeverRemove
                    });
                }
                else
                {
                    product.Retained = cachedProduct.Retained;
                }

            }
            return products;
        }

        /// <summary>
        /// Attempt to update the retained quantity of a given product
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="productId"></param>
        /// <param name="product"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool TryUpdateRetained(int quantity, int productId, out Product product, out CartOperationError error)
        {
            error = new CartOperationError();
            product = null;
            var productCacheKey = ProductCacheKey(productId);
            if (!memoryCache.TryGetValue(productCacheKey, out product))
            {
                error.StatusCode = 404;
                error.Message = "Product not found";
                return false;
            }

            var remaining = product.Stock - product.Retained;

            if (quantity > 0 && quantity > remaining)
            {
                error.StatusCode = 400;
                error.Message = "Quantity too large requested";
                return false;
            }

            product.Retained += quantity;
            memoryCache.Set(productCacheKey, product);

            return true;
        }

        public void SetProduct(Product product)
        {
            var productCacheKey = ProductCacheKey(product.Id);
            memoryCache.Set(productCacheKey, product);
        }
    }
}
