using System.Collections.Generic;
using Checkout.Api.Carts.Services;
using Checkout.Api.Products.Models;

namespace Checkout.Api.Products.Services
{
    public interface IProductCacheService
    {
        Product FindCached(int productId);
        List<Product> List();
        void StoreCachedProduct(Product product);
        bool TryUpdateRetained(int quantity, int productId, out Product product, out CartOperationError error);
    }
}