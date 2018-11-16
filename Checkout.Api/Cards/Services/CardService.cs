using System;
using System.Collections.Generic;
using Checkout.Api.Cards.Controllers;
using Microsoft.Extensions.Caching.Memory;

namespace Checkout.Api.Cards.Services
{
    public class CardService
    {
        private readonly IMemoryCache memoryCache;

        public CardService(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        public Card CreateCard()
        {
            var card = new Card
            {
                Id = Guid.NewGuid().ToString("N"),
                CardItems = new List<CardItem>()
            };
            return card;
        }
    }
}
