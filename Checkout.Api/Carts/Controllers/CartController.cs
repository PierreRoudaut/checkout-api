using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Checkout.Api.Carts.Models;
using Checkout.Api.Carts.Services;

namespace Checkout.Api.Carts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly CartService service;

        public CartController(CartService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Fetch a given Cart
        /// </summary>
        /// <returns></returns>
        [HttpGet("{cartId}", Name = "Cart")]
        [ProducesResponseType(typeof(Cart), 200)]
        public ObjectResult GetCart(string cartId = null)
        {
            var cart = service.GetOrCreateCart(cartId);
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
            if (!service.ClearCart(cartId))
            {
                return StatusCode(404, new
                {
                    Message = "Cart not found"
                });
            }
            return StatusCode(200, true);
        }

        /// <summary>
        /// Adds or replace a CartItem into a given Cart
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cartId}/setItem", Name = "SetItemToCart")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult SetItem([Required] string cartId, [FromBody] CartItem cartItem)
        {
            if (!service.SetCartItem(cartId, cartItem, out var error))
            {
                return StatusCode(error.StatusCode, new { error.Message });
            }
            return StatusCode(200, true);
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
            if (!service.RemoveCartItem(cartId, productId))
            {
                return StatusCode(404, new { Message = "Cart not found" });
            }
            return StatusCode(200, true);
        }
    }
}