using System.Collections.Generic;
using System.Linq;
using Checkout.Api.Core.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Checkout.Api.Products.Models
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext dbContext;

        public ProductRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Retrieve all products
        /// </summary>
        /// <returns></returns>
        public List<Product> GetAllProducts()
        {
            var list = dbContext.Products.OrderBy(x => x.Name).ToList();
            return list;
        }

        /// <summary>
        /// Create a given product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool CreateProduct(Product product)
        {
            try
            {
                dbContext.Products.Add(product);
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Log.Error("Failed to create product: " + product.Name);
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Updates a product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool UpdateProduct(Product product)
        {
            try
            {
                dbContext.Entry(product).State = EntityState.Modified;
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Log.Error("Failed to update product: " + product.Name);
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes a given product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool Delete(Product product)
        {
            try
            {
                dbContext.Entry(product).State = EntityState.Deleted;
                dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                Log.Error("Failed to delete product: " + product.Name);
                Log.Error(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Finds a product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Product Find(int id) => dbContext.Products.FirstOrDefault(c => c.Id == id);

        /// <summary>
        /// Check if a given product with id exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exists(int id) => dbContext.Products.Any(c => c.Id == id);
    }
}