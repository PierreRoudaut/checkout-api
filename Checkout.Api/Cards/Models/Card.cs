using System.Collections.Generic;
using Checkout.Api.Cards.Models;

namespace Checkout.Api.Cards.Models
{
    public class Card
    {
        public string Id { get; set; }
        public Dictionary<int, CardItem> CardItems { get; set; }
    }
}