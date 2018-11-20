using System;
using Checkout.Api.Products.Models;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Checkout.Api.Carts.Models;
using Checkout.Api.Carts.Services;
using Checkout.Api.Hubs;
using Microsoft.AspNetCore.SignalR;


namespace Checkout.Api.Tests.Carts
{
    [TestFixture]
    public class CartServiceTests
    {
        private Mock<IProductRepository> repository;
        private Mock<IMemoryCache> memoryCache;
        private Mock<ICacheEntry> cacheEntry;
        private Mock<IHubContext<NotifyHub>> hub;

        [SetUp]
        public void Setup()
        {
            hub = new Mock<IHubContext<NotifyHub>>();
            cacheEntry = new Mock<ICacheEntry>();
            memoryCache = new Mock<IMemoryCache>();
            memoryCache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(cacheEntry.Object);
            repository = new Mock<IProductRepository>();
        }

        [TestCase("not-found", false)]
        [TestCase("found", true)]
        public void GetOrCreateCartTest(string cartId, bool found)
        {
            var cart = new Cart();

            var obj = (object)cart;
            memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(found);

            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);
            var retrievedcart = service.GetCart(cartId);
            Assert.AreEqual(found ? cart : null, retrievedcart);
        }

        [Test]
        public void ClearCartTestCaseNotFound()
        {
            var cart = new Cart();
            var obj = (object)cart;

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(false);
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);
            Assert.Null(service.ClearCart("123"));
        }

        [Test]
        public void ClearCartTestCaseOk()
        {
            var cart = new Cart
            {
                CartItems = new Dictionary<int, CartItem> { { 42, new CartItem() } }
            };
            var obj = (object)cart;

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(true);
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);

            Assert.NotNull(service.ClearCart("123"));
            Assert.IsEmpty(cart.CartItems);
        }



        [Test]
        public void SetCartItemCaseInvalidQuantity()
        {
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);
            var error = new CartOperationError();
            Assert.Null(service.SetCartItem("123", new CartItem { Quantity = 0 }, out error));
            Assert.AreEqual(400, error.StatusCode);
        }


        [Test]
        public void SetCartItemCaseProductNotFound()
        {
            repository.Setup(x => x.Find(It.IsAny<int>())).Returns((Product)null);
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);
            var error = new CartOperationError();
            Assert.Null(service.SetCartItem("123", new CartItem { Quantity = 1, ProductId = 42 }, out error));
            Assert.AreEqual(400, error.StatusCode);
        }


        [Test]
        public void SetCartItemCasecartNotFound()
        {
            //todo: reimplement

            //var cart = new Cart();
            //var product = new Product();
            //repository.Setup(x => x.Find(It.IsAny<int>())).Returns(product);
            //var obj = (object)cart;

            //memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(false);
            //var service = new CartService(memoryCache.Object, repository.Object, hub.Object);
            //var error = new CartOperationError();
            //Assert.Null(service.SetCartItem("123", new CartItem { Quantity = 1, ProductId = 42 }, out error));
            //Assert.AreEqual(404, error.StatusCode);
        }


        [Test]
        public void SetCartItemCaseOk()
        {
            //var cart = new Cart
            //{
            //    Id = "123",
            //    CartItems = new Dictionary<int, CartItem> { { 42, new CartItem { Quantity = 1, ProductId = 42 } } }
            //};
            //var product = new Product();
            //var obj = (object)cart;

            //repository.Setup(x => x.Find(It.IsAny<int>())).Returns(product);
            //memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(true);
            //var service = new CartService(memoryCache.Object, repository.Object, hub.Object);

            //var error = new CartOperationError();
            //Assert.NotNull(service.SetCartItem("123", new CartItem { Quantity = 3, ProductId = 42 }, out error));
            //Assert.AreEqual(1, cart.CartItems.Count);
            //Assert.AreEqual(3, cart.CartItems.First().Value.Quantity);
            //Assert.NotNull(service.SetCartItem("123", new CartItem { Quantity = 1, ProductId = 43 }, out error));
            //Assert.AreEqual(2, cart.CartItems.Count);
        }

        [Test]
        public void RemoveCartItemTestCaseNotFound()
        {
            var cart = new Cart();
            var obj = (object)cart;

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(false);
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);
            Assert.Null(service.RemoveCartItem("123", 42));
        }

        [Test]
        public void RemoveCartItemTesCaseOk()
        {
            var cart = new Cart
            {
                Id = "123",
                CartItems = new Dictionary<int, CartItem> {
                    { 42, new CartItem { ProductId = 42, Quantity = 2 } },
                    { 43, new CartItem { ProductId = 43, Quantity = 2 } }
                }
            };
            var obj = (object)cart;

            memoryCache.Setup(x => x.TryGetValue(It.IsAny<string>(), out obj)).Returns(true);
            var service = new CustomerCartService(memoryCache.Object, repository.Object, hub.Object);

            Assert.NotNull(service.RemoveCartItem("123", 42));
            Assert.AreEqual(1, cart.CartItems.Count);
        }
    }
}
