using Checkout.Api.Cards.Models;
using Checkout.Api.Cards.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Checkout.Api.Cards.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        private readonly CardService service;

        public CardController(CardService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Fetch a given Card
        /// </summary>
        /// <returns></returns>
        [HttpGet("{cardId}", Name = "Card")]
        [ProducesResponseType(typeof(Card), 200)]
        public ObjectResult GetCard([Required] string cardId = null)
        {
            var card = service.GetOrCreateCard(cardId);
            return StatusCode(200, card);
        }

        /// <summary>
        /// Empty all items of a given card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/clear", Name = "ClearCard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult ClearCard([Required] string cardId)
        {
            if (!service.ClearCard(cardId))
            {
                return StatusCode(404, new
                {
                    Message = "Card not found"
                });
            }
            return StatusCode(200, true);
        }

        /// <summary>
        /// Adds or replace a CardItem into a given Card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/setItem", Name = "SetItemToCard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult SetItem([Required] string cardId, [FromBody] CardItem cardItem)
        {
            if (!service.SetCardItem(cardId, cardItem, out var error))
            {
                return StatusCode(error.StatusCode, new { error.Message });
            }
            return StatusCode(200, true);
        }

        /// <summary>
        /// Empty all items of a given card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/removeItem", Name = "RemoveItemFromCard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult RemoveItem([Required] string cardId, [FromBody] CardItem cardItem)
        {
            if (!service.RemoveCardItem(cardId, cardItem))
            {
                return StatusCode(404, new {Message = "Card not found"});
            }
            return StatusCode(200, true);
        }
    }
}