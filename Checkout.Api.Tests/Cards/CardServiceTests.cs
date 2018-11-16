using Checkout.Api.Cards.Models;
using Checkout.Api.Cards.Services;
using Checkout.Api.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;


namespace Checkout.Api.Tests.Cards
{
    [TestFixture]
    public class CardServiceTests
    {
        private Mock<ProductRepository> repository;
        private Mock<IMemoryCache> memoryCache;

        [SetUp]
        public void Setup()
        {
            memoryCache = new Mock<IMemoryCache>();
            repository = new Mock<ProductRepository>();
        }

        [TestCase(null, 0)]
        [TestCase("not-found", 0)]
        [TestCase("found", 1)]
        public void GetOrCreateCardTest(string cardId, int nbItems)
        {
            var card = new Card();
            if (nbItems > 0)
            {
                card.CardItems.Add(42, new CardItem());
            }

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(false);
            var service = new CardService(memoryCache.Object, repository.Object);
            var retrievedCard = service.GetOrCreateCard(cardId);

            Assert.AreEqual(nbItems, retrievedCard.CardItems.Count);
        }

        [Test]
        public void ClearCardTestCaseNotFound()
        {
            var card = new Card();
            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(false);
            var service = new CardService(memoryCache.Object, repository.Object);
            Assert.IsFalse(service.ClearCard("123"));
        }

        [Test]
        public void ClearCardTestCaseOk()
        {
            var card = new Card
            {
                CardItems = new Dictionary<int, CardItem> { { 42, new CardItem() } }
            };

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(false);
            var service = new CardService(memoryCache.Object, repository.Object);

            Assert.IsTrue(service.ClearCard("123"));
            Assert.IsEmpty(card.CardItems);
        }



        [Test]
        public void SetCardItemCaseInvalidQuantity()
        {
            var service = new CardService(memoryCache.Object, repository.Object);
            var error = new CardOperationError();
            Assert.IsFalse(service.SetCardItem("123", new CardItem { Quantity = 0 }, out error));
            Assert.AreEqual(400, error.StatusCode);
        }


        [Test]
        public void SetCardItemCaseProductNotFound()
        {
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns((Product)null);
            var service = new CardService(memoryCache.Object, repository.Object);
            var error = new CardOperationError();
            Assert.IsFalse(service.SetCardItem("123", new CardItem { Quantity = 1, ProductId = 42 }, out error));
            Assert.AreEqual(400, error.StatusCode);
        }


        [Test]
        public void SetCardItemCaseCardNotFound()
        {
            var card = new Card();
            var product = new Product();
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns(product);
            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(false);
            var service = new CardService(memoryCache.Object, repository.Object);
            var error = new CardOperationError();
            Assert.IsFalse(service.SetCardItem("123", new CardItem { Quantity = 1, ProductId = 42 }, out error));
            Assert.AreEqual(404, error.StatusCode);
        }


        [Test]
        public void SetCardItemCaseOk()
        {
            var card = new Card
            {
                Id = "123",
                CardItems = new Dictionary<int, CardItem> { { 42, new CardItem { Quantity = 1, ProductId = 42 } } }
            };
            var product = new Product();
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns(product);
            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(true);
            var service = new CardService(memoryCache.Object, repository.Object);
            var error = new CardOperationError();
            Assert.IsTrue(service.SetCardItem("123", new CardItem { Quantity = 3, ProductId = 42 }, out error));
            Assert.AreEqual(1, card.CardItems.Count);
            Assert.AreEqual(3, card.CardItems.First().Value.Quantity);
            Assert.IsTrue(service.SetCardItem("123", new CardItem { Quantity = 1, ProductId = 43 }, out error));
            Assert.AreEqual(2, card.CardItems.Count);
        }

        [Test]
        public void RemoveCardItemTestCaseNotFound()
        {
            var card = new Card();
            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(false);
            var service = new CardService(memoryCache.Object, repository.Object);
            Assert.IsFalse(service.RemoveCardItem("123", new CardItem()));
        }

        [Test]
        public void RemoveCardItemTesCaseOk()
        {
            var card = new Card
            {
                CardItems = new Dictionary<int, CardItem> {
                    { 42, new CardItem { ProductId = 42, Quantity = 2 } },
                    { 43, new CardItem { ProductId = 43, Quantity = 2 } }
                }
            };

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out card)).Returns(true);
            var service = new CardService(memoryCache.Object, repository.Object);

            Assert.IsTrue(service.RemoveCardItem("123", new CardItem { ProductId = 42, Quantity = 2 }));
            Assert.AreEqual(1, card.CardItems.Count);
        }
    }
}
