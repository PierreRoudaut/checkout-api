using Checkout.Api.Cards.Models;
using Checkout.Api.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

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
        private readonly IProductRepository productRepository;

        private const string CardKeyFormat = "[card][{0}]";

        public static MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        public CardService(IMemoryCache memoryCache, IProductRepository productRepository)
        {
            this.memoryCache = memoryCache;
            this.productRepository = productRepository;
        }

        /// <summary>
        /// Retrieves an existing card or create a new one
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public Card GetOrCreateCard(string cardId = null)
        {
            if (cardId != null && memoryCache.TryGetValue(string.Format(CardKeyFormat, cardId), out Card card))
            {
                return card;
            }

            card = new Card
            {
                Id = Guid.NewGuid().ToString("N"),
                CardItems = new Dictionary<int, CardItem>()
            };
            var entry = memoryCache.CreateEntry(string.Format(CardKeyFormat, card.Id));
            entry.Value = card;
            entry.SlidingExpiration = TimeSpan.FromMinutes(10);

            //memoryCache.Set(string.Format(CardKeyFormat, card.Id), card, CacheEntryOptions);
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
            memoryCache.Set(cardId, card, CacheEntryOptions);
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
            memoryCache.Set(cardId, card, CacheEntryOptions);
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
            memoryCache.Set(cardId, card, CacheEntryOptions);
            return true;
        }
    }
}