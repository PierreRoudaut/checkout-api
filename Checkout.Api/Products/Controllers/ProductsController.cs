using Checkout.Api.Products.Models;
using Checkout.Api.Products.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Checkout.Api.Products.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository repository;
        private readonly ProductImageService productImageService;

        public ProductsController(IProductRepository repository, ProductImageService productImageService)
        {
            this.repository = repository;
            this.productImageService = productImageService;
        }

        /// <summary>
        /// Enforce image upon product creation
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost("create", Name = "CreateProduct")]
        [ProducesResponseType(typeof(Product), 201)]
        [ProducesResponseType(400)]
        public ObjectResult CreateProduct([FromForm] Product product)
        {
            if (product.Image == null || !productImageService.IsImageValid(product.Image))
            {
                return StatusCode(400, new
                {
                    Message = "The image should be a valid png/jpeg lower than 10 MB"
                });
            }

            product.ImageFilename = Guid.NewGuid().ToString("N") + Path.GetExtension(product.Image.FileName);

            if (!repository.CreateProduct(product))
            {
                return StatusCode(400, new
                {
                    Message = "Failed to create user"
                });
            }

            productImageService.SaveProductImage(product.Image, product.ImageFilename);

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
        public ObjectResult UpdateProduct([FromForm] Product product)
        {
            var existingProduct = repository.Find(product.Id);
            if (existingProduct == null)
            {
                return StatusCode(404, new
                {
                    Message = "Product not found"
                });
            }
            var oldFilename = existingProduct.ImageFilename;


            if (product.Image != null)
            {
                if (!productImageService.IsImageValid(product.Image))
                {
                    return StatusCode(400, new
                    {
                        Message = "The image should be a valid png/jpeg lower than 10 MB"
                    });
                }
                oldFilename = product.ImageFilename;
                product.ImageFilename = Guid.NewGuid().ToString("N") + Path.GetExtension(product.Image.FileName);
            }

            // updating product
            if (!repository.UpdateProduct(product))
            {
                return StatusCode(400, new
                {
                    Message = "Failed to update user"
                });
            }

            // persisting new image
            if (product.Image != null)
            {
                productImageService.DeleteProductImage(oldFilename);
                productImageService.SaveProductImage(product.Image, product.ImageFilename);
            }

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

            productImageService.DeleteProductImage(product.ImageFilename);
            return StatusCode(200, true);
        }

        [HttpGet("", Name = "GetAllProducts")]
        [ProducesResponseType(typeof(List<Product>), 200)]
        public ObjectResult GetProducts()
        {
            var products = repository.GetAllProducts();
            return Ok(products);
        }
    }
}