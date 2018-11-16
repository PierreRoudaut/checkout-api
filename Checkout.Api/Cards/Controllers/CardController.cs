using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Checkout.Api.Products.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.Api.Cards.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardController : ControllerBase
    {
        /// <summary>
        /// Fetch a given Card
        /// </summary>
        /// <returns></returns>
        [HttpPut("new", Name = "Card")]
        [ProducesResponseType(typeof(string), 201)]
        public ObjectResult NewCard()
        {
            return StatusCode(200, product);
        }


        /// <summary>
        /// Fetch a given Card
        /// </summary>
        /// <returns></returns>
        [HttpGet("{cardId}", Name = "Card")]
        [ProducesResponseType(typeof(Card), 200)]
        [ProducesResponseType(404)]
        public ObjectResult GetCard([Required] string cardId)
        {
            return StatusCode(200, product);
        }

        /// <summary>
        /// Empty all items of a given card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/empty", Name = "EmptyCard")]
        [ProducesResponseType(typeof(Card), 200)]
        [ProducesResponseType(404)]
        public ObjectResult EmptyCard([Required] string cardId)
        {
            return StatusCode(200, product);
        }

        /// <summary>
        /// Empty all items of a given card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/addItem", Name = "AddItemToCard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult AddItem([FromBody] CardItem cardItem)
        {
            return StatusCode(200, product);
        }

        /// <summary>
        /// Empty all items of a given card
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/removeItem", Name = "RemoveItemFromCard")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult RemoveItem([FromBody] CardItem cardItem)
        {
            return StatusCode(200, product);
        }

        /// <summary>
        /// Update card item quantity
        /// </summary>
        /// <returns></returns>
        [HttpPost("{cardId}/updateItemQuantity", Name = "UpdateItemQuantity")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public ObjectResult UpdateItemQuantity([FromBody] CardItem cardItem)
        {
            return StatusCode(200, product);
        }
    }

    public class Card
    {
        public string Id { get; set; }
        public List<CardItem> CardItems { get; set; }
    }

    public class CardItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}