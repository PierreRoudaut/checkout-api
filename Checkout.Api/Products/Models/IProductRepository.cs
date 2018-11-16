using System.Collections.Generic;

namespace Checkout.Api.Products.Models
{
    public interface IProductRepository
    {
        bool CreateProduct(Product product);
        Product Find(int id);
        List<Product> GetAllProducts();
        bool UpdateProduct(Product product);
    }
}