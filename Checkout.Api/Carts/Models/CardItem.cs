using System.Collections.Generic;

namespace Checkout.Api.Carts.Models
{
    public class CartItem : IEqualityComparer<CartItem>
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public bool Equals(CartItem x, CartItem y) => x.ProductId == y.ProductId;

        public int GetHashCode(CartItem obj) => obj.ProductId * obj.Quantity;
    }
}