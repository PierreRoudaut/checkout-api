using Checkout.Api.Cards.Controllers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Checkout.Api.Cards.Models;
using Checkout.Api.Products.Models;

namespace Checkout.Api.Cards.Services
{
    public class CardOperationError
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
    }

    public class CardService
    {
        private readonly IMemoryCache memoryCache;
        private readonly ProductRepository productRepository;

        private const string CardKeyFormat = "[card][{0}]";

        public CardService(IMemoryCache memoryCache, ProductRepository productRepository)
        {
            this.memoryCache = memoryCache;
            this.productRepository = productRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public Card GetOrCreateCard(string cardId = null)
        {
            if (cardId != null && memoryCache.TryGetValue(string.Format(CardKeyFormat, cardId), out Card card))
            {
                return card;
            }

            // if card is expired we create a new card for
            card = new Card
            {
                Id = Guid.NewGuid().ToString("N"),
                CardItems = new Dictionary<int, CardItem>()
            };
            memoryCache.Set(string.Format(CardKeyFormat, card.Id), card, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            });
            return card;
        }

        /// <summary>
        /// Clears the items of a given card
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public bool ClearCard(string cardId)
        {
            if (!memoryCache.TryGetValue(string.Format(CardKeyFormat, cardId), out Card card))
            {
                return false;
            }
            card.CardItems.Clear();
            memoryCache.Set(cardId, card);
            return true;
        }

        /// <summary>
        /// Adds or replace (if different quantity) a card item
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="item"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public bool SetCardItem(string cardId, CardItem item, out CardOperationError error)
        {
            error = new CardOperationError();
            if (item.Quantity <= 0)
            {
                error.StatusCode = 400;
                error.Message = "Invalid quantity";
                return false;
            }

            var product = productRepository.Find(item.ProductId);
            if (product == null)
            {
                error.StatusCode = 400;
                error.Message = "Product does not exists";
                return false;
            }
            if (!memoryCache.TryGetValue(string.Format(CardKeyFormat, cardId), out Card card))
            {
                error.StatusCode = 404;
                error.Message = "Card not found";
                return false;
            }
            card.CardItems[item.ProductId] = item;
            memoryCache.Set(cardId, card);
            return true;
        }

        /// <summary>
        /// Removes an item for a given card
        /// </summary>
        /// <param name="cardId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool RemoveCardItem(string cardId, CardItem item)
        {
            if (!memoryCache.TryGetValue(string.Format(CardKeyFormat, cardId), out Card card))
            {
                return false;
            }

            card.CardItems.Remove(item.ProductId);
            memoryCache.Set(cardId, card);
            return true;
        }
    }
}
