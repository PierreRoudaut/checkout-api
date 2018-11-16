using System.Collections.Generic;

namespace Checkout.Api.Cards.Models
{
    public class CardItem : IEqualityComparer<CardItem>
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool Equals(CardItem x, CardItem y) => x.ProductId == y.ProductId;

        public int GetHashCode(CardItem obj) => obj.ProductId * obj.Quantity;
    }
}