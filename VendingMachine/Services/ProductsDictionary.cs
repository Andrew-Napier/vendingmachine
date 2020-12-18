using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendingMachine.Models;

namespace VendingMachine
{
    public interface IProductsDictionary
    {
        void Add(Product product);
        List<Product> GetAll();
        public Product GetItem(string productId);

        void Update(Product product);
    }

    public class ProductsDictionary : IProductsDictionary
    {
        private readonly Dictionary<string, Product> products 
            = new Dictionary<string, Product>();

        public void Add(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (string.IsNullOrEmpty(product.Id))
            {
                throw new ArgumentException("Product ID cannot be empty.");
            }

            products.Add(product.Id, product);
        }

        public void Update(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (string.IsNullOrEmpty(product.Id) || !products.ContainsKey(product.Id) )
            {
                throw new ArgumentException("Invalid Product ID.");
            }

            products[product.Id] = product;
        }

        public List<Product> GetAll()
        { 
        
            var allProducts = new List<Product>();
            allProducts.AddRange(products.Values);

            return allProducts;
        }

        public Product GetItem(string productId)
        {
            return products[productId];
        }
    }

    public static class ProductsDictionaryExtensions
    {
        public static IServiceCollection InitialiseProductsDictionary(this IServiceCollection services)
            => services.AddSingleton<IProductsDictionary>(_ => 
                {
                    var p = new ProductsDictionary();
                    p.Add(new Product { 
                                Id = "coke", 
                                Description = "Coca Cola 375ml", 
                                Price = 1.50M, 
                                Quantity = 25 });
                    p.Add(new Product { 
                                Id = "almonds", 
                                Description = "Natural Almonds 250g", 
                                Price = 2.75M, 
                                Quantity = 5 });
                    p.Add(new Product
                    {
                        Id = "handcream",
                        Description = "Vitamin E Cream 100g",
                        Price = 4.95M,
                        Quantity = 5
                    });

                    return p; 
                });
    }
}