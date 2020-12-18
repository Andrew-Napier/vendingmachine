using System;

namespace VendingMachine.Models
{
    public class Product
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public Decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
