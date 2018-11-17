using System.Collections.Generic;

namespace Checkout.Api.Carts.Models
{
    public class Cart
    {
        public string Id { get; set; }
        public Dictionary<int, CartItem> CartItems { get; set; }
    }
}