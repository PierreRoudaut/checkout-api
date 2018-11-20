using Checkout.Api.Carts.Models;
using Checkout.Api.Carts.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Checkout.Api.Carts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly CustomerCartService service;

        public CartController(CustomerCartService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Fetch a given Cart
        /// </summary>
        /// <returns></returns>
        [HttpGet("{cartId}", Name = "Cart")]
        [ProducesResponseType(typeof(Cart), 200)]
        [ProducesResponseType(404)]
        public ObjectResult GetCart(string cartId = null)
        {
            var cart = service.GetCart(cartId);
            if (cart == null)
            {
                return StatusCode(404, new
                {
                    Message = "Cart not found"
                });
            }
            return StatusCode(200, cart);
        }

        /// <summary>
        /// Clear all items of a given cart
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cartId}/clear", Name = "ClearCart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult ClearCart([Required] string cartId)
        {
            var cart = service.ClearCart(cartId);
            if (cart == null)
            {
                return StatusCode(404, new
                {
                    Message = "Cart not found"
                });
            }
            return StatusCode(200, cart);
        }

        /// <summary>
        /// Adds or replace a CartItem into a given Cart
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cartId}/setItem", Name = "SetItemToCart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult SetItem(string cartId, [FromBody] CartItem cartItem)
        {
            var cart = service.SetCartItem(cartId, cartItem, out var error);
            if (cart == null)
            {
                return StatusCode(error.StatusCode, new { error.Message });
            }
            return StatusCode(200, cart);
        }

        /// <summary>
        /// Empty all items of a given cart
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{cartId}/removeItem/{productId}", Name = "RemoveItemFromCart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult RemoveItem([Required] string cartId, [Required] int productId)
        {
            var cart = service.RemoveCartItem(cartId, productId);
            if (cart == null)
            {
                return StatusCode(404, new { Message = "Cart not found" });
            }
            return StatusCode(200, cart);
        }
    }
}