using System;
using System.Collections.Generic;

namespace VendingMachine.Models
{
    public class ShoppingCart
    {
        public Decimal RemainingFunds { get; set; }
        public List<Product> PurchasedItems { get; }
        public int UserId { get; }

        public ShoppingCart(int cartId)
        {
            UserId = cartId;
            PurchasedItems = new List<Product>();
        }
    }
}
