using Checkout.Api.Products.Models;
using Checkout.Api.Products.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Checkout.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Checkout.Api.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository repository;
        private readonly ProductImageService productImageService;
        private readonly IProductCacheService productCacheService;
        private readonly IHubContext<NotifyHub> hubContext;

        public ProductsController(IProductRepository repository, ProductImageService productImageService, IProductCacheService productCacheService, IHubContext<NotifyHub> hubContext)
        {
            this.repository = repository;
            this.productImageService = productImageService;
            this.productCacheService = productCacheService;
            this.hubContext = hubContext;
        }

        /// <summary>
        /// Enforce image upon product creation
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("create", Name = "CreateProduct")]
        [ProducesResponseType(typeof(Product), 201)]
        [ProducesResponseType(400)]
        public ObjectResult CreateProduct([FromBody] Product product)
        {
            if (!productImageService.IsImageValid(product.ImageUrl))
            {
                return StatusCode(400, new
                {
                    Message = "The image should be a valid png/jpeg smaller than 10 MB"
                });
            }

            if (!repository.CreateProduct(product))
            {
                return StatusCode(400, new
                {
                    Message = "Failed to create user"
                });
            }

            productCacheService.SetProduct(product);
            hubContext.Clients?.All?.SendAsync(AppEvents.ProductUpdated, product).Wait();
            return StatusCode(201, product);
        }

        /// <summary>
        /// Product image is optionnal upon update
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("update", Name = "UpdateProduct")]
        [ProducesResponseType(typeof(Product), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public ObjectResult UpdateProduct([FromBody] Product product)
        {
            if (!repository.Exists(product.Id))
            {
                return StatusCode(404, new
                {
                    Message = "Product not found"
                });
            }

            if (!productImageService.IsImageValid(product.ImageUrl))
            {
                return StatusCode(400, new
                {
                    Message = "The image should be a valid png/jpeg smaller than 10 MB"
                });
            }

            // updating product
            if (!repository.UpdateProduct(product))
            {
                return StatusCode(400, new
                {
                    Message = "Failed to update user"
                });
            }
            hubContext.Clients?.All?.SendAsync(AppEvents.ProductUpdated, product).Wait();
            return StatusCode(200, product);
        }

        /// <summary>
        /// Delete a product as well as its image resource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}", Name = "DeleteProduct")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public ObjectResult DeleteProduct([Required] int id)
        {
            var product = productCacheService.List().FirstOrDefault(x => x.Id == id);
            if (product == null)
            {
                return StatusCode(404, new
                {
                    Message = "Product not found"
                });
            }
            // preventing product deletion if currently stored in at least 1 cart
            if (product.Retained > 0)
            {
                return StatusCode(400, new
                {
                    Message = $"{product.Retained} items of this product are currently retained in carts"
                });
            }

            if (!repository.Delete(product))
            {
                return StatusCode(400, new { Message = "Failed to delete product" });
            }
            hubContext.Clients?.All?.SendAsync(AppEvents.ProductDeleted, product).Wait();
            return StatusCode(200, true);
        }

        [HttpGet("", Name = "GetAllProducts")]
        [ProducesResponseType(typeof(List<Product>), 200)]
        public ObjectResult GetProducts()
        {
            var products = productCacheService.List();
            return Ok(products);
        }
    }
}