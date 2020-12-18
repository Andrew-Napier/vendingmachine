using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VendingMachine.Models;

namespace VendingMachine.Services
{
    public interface IShoppingCartFactory
    {
        public ShoppingCart AddOrRetrieveCart(int cartId);
        public void RemoveCart(int cartId);
    }

    public class ShoppingCartFactory : IShoppingCartFactory
    {
        private readonly Dictionary<int, ShoppingCart> carts 
            = new Dictionary<int, ShoppingCart>();
        private readonly ILogger<ShoppingCartFactory> log;

        public ShoppingCartFactory(ILogger<ShoppingCartFactory> logger) => 
            log = logger ?? throw new ArgumentNullException(nameof(logger));

        public ShoppingCart AddOrRetrieveCart(int cartId)
        {
            log.LogInformation("Requesting Cart: {cartId}");
            if (carts.ContainsKey(cartId))
            {
                return carts[cartId];
            } 
            else
            {
                log.LogInformation("New cart created for: {cartId}");
                var cart = new ShoppingCart(cartId);
                carts.Add(cartId, cart);

                return cart;
            }
        }

        public void RemoveCart(int cartId)
        {
            if (carts.ContainsKey(cartId))
            {
                log.LogInformation("Removing cart: {cartId}");
                carts.Remove(cartId);
            }
        }
    }

}
