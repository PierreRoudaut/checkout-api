using System.Collections.Generic;
using System.Linq;
using Checkout.Api.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Checkout.Api.Products.Models
{
    public class ProductRepository
    {
        private readonly AppDbContext dbContext;

        public ProductRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<Product> GetAllProducts()
        {
            var list = dbContext.Products.OrderBy(x => x.Name).ToList();
            return list;
        }

        public bool CreateProduct(Product product)
        {
            try
            {
                dbContext.Products.Add(product);
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Log.Error("Failed to add user: " + product.Name);
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                dbContext.Entry(product).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Log.Error("Failed to update user: " + product.Name);
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }

        public Product Find(int id) => dbContext.Products.FirstOrDefault(c => c.Id == id);
    }
}