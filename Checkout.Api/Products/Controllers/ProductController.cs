using System.Collections.Generic;
using Checkout.Api.Products.Models;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.Api.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository repository;

        public ProductController(ProductRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet("", Name = "GetAllProducts")]
        [ProducesResponseType(typeof(List<Product>), 201)]
        public ObjectResult GetProducts([FromBody] string fileUrl)
        {
            var products = repository.GetAllProducts();
            return Ok(products);
        }
    }
}