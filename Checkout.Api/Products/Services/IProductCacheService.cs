using System.Collections.Generic;
using Checkout.Api.Carts.Services;
using Checkout.Api.Products.Models;

namespace Checkout.Api.Products.Services
{
    public interface IProductCacheService
    {
        List<Product> List();
        bool TryUpdateRetained(int quantity, int productId, out Product product, out CartOperationError error);
        void SetProduct(Product product);
    }
}