using Checkout.Api.Products.Models;
using Checkout.Api.Products.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            // todo: check if a product is currently stored in a cart
            var product = repository.Find(id);
            if (product == null)
            {
                return StatusCode(404, new
                {
                    Message = "Product not found"
                });
            }

            if (!repository.Delete(product))
            {
                return StatusCode(400, new { Message = "Failed to delete product" });
            }

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