using System;
using VendingMachine.Models;

namespace VendingMachine.Services
{
    public interface IShoppingCartManipulator
    {
        void AddPayment(ShoppingCart cart, decimal moneyAdded);
        void AddItem(Product product, ShoppingCart cart);
    }

    public class ShoppingCartManipulator : IShoppingCartManipulator
    {
        public void AddItem(Product product, ShoppingCart cart)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (cart == null)
            {
                throw new ArgumentNullException(nameof(cart));
            }

            cart.PurchasedItems.Add(product);
            cart.RemainingFunds -= product.Price;
        }

        public void AddPayment(ShoppingCart cart, decimal moneyAdded)
        {
            if (cart is null)
            {
                throw new ArgumentNullException(nameof(cart));
            }
            if (moneyAdded < 0M)
            {
                throw new ArgumentException("Unable to add negative funds to cart.");
            }

            cart.RemainingFunds += moneyAdded;
        }
    }
}
