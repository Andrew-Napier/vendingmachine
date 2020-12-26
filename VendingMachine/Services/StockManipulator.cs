using System;
using VendingMachine.Models;

namespace VendingMachine.Services
{
    public interface IStockManipulator
    {
        bool IsValidPurchaseRequest(Product product, decimal remainingFunds);
        void ReduceStockLevel(Product product);
    }

    public class StockManipulator : IStockManipulator
    {
        private readonly IProductsDictionary products;

        public StockManipulator(IProductsDictionary productsDictionary)
        {
            products = productsDictionary ?? throw new ArgumentNullException(nameof(productsDictionary));
        }

        public bool IsValidPurchaseRequest(Product product, decimal remainingFunds)
        {
            return product != null
                && product.Quantity > 0
                && product.Price > 0.0M
                && remainingFunds >= product.Price;
        }

        public void ReduceStockLevel(Product product)
        {
            var revisedProduct = products.GetItem(product.Id);
            --revisedProduct.Quantity;

            products.Update(revisedProduct);
        }
    }
}
