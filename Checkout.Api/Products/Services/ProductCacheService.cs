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


        public ProductCacheService(IProductRepository repository, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
        }

        public List<Product> List()
        {
            var products = repository.GetAllProducts();
            foreach (var product in products)
            {
                var productCacheKey = string.Format(ProductCacheKeyFormat, product.Id);
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

        public Product FindCached(int productId)
        {
            var productCacheKey = string.Format(ProductCacheKeyFormat, productId);
            if (!memoryCache.TryGetValue(productCacheKey, out Product cachedProduct))
            {
                return null;
            }

            return cachedProduct;
        }

        public void StoreCachedProduct(Product product)
        {
            var productCacheKey = string.Format(ProductCacheKeyFormat, product.Id);
            memoryCache.Set(productCacheKey, product);
        }

        public bool TryUpdateRetained(int quantity, int productId, out Product product, out CartOperationError error)
        {
            error = null;
            product = FindCached(productId);

            var remaining = product.Stock - product.Retained;

            if (quantity > 0 && quantity > remaining)
            {
                error = new CartOperationError
                {
                    StatusCode = 400,
                    Message = "Quantity too large requested"
                };
                return false;
            }

            product.Retained += quantity;
            StoreCachedProduct(product);

            return true;
        }
    }
}
