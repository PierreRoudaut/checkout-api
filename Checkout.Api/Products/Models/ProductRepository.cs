using System.Collections.Generic;
using System.Text;

namespace Checkout.Api.Products.Models
{
    public class ProductRepository
    {
        public List<Product> GetAllProducts()
        {
            var list = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Bazooka",
                    Image = null,
                    Description = "blbabla"
                },
                new Product
                {
                    Id = 2,
                    Name = "Uzi",
                    Image = null,
                    Description = "blabla"
                }
            };
            return list;
        }
    }
}