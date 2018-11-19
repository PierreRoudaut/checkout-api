using System.Collections.Generic;

namespace Checkout.Api.Products.Models
{
    public interface IProductRepository
    {
        bool CreateProduct(Product product);
        Product Find(int id);
        List<Product> GetAllProducts();
        bool UpdateProduct(Product product);
        bool Delete(Product product);

        /// <summary>
        /// Check if a given product with id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool Exists(int id);
    }
}