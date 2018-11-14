using System;
using Checkout.Api.Products.Models;
using NUnit.Framework;

namespace Checkout.Api.Tests.Products.Models
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        [Test]
        public void GetAllProductTest()
        {
            var repo = new ProductRepository();
            var products = repo.GetAllProducts();

            Assert.Less(0, products.Count);
        }
    }
}
