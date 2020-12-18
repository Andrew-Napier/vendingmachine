using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using VendingMachine.Models;
using VendingMachine.Services;

namespace VendingMachine
{
    public class HttpTriggers
    {
        public IProductsDictionary Products { get; }
        public IShoppingCartFactory ShoppingCartFactory { get; }
        public IShoppingCartManipulator CartManipulator { get; }
        public IStockManipulator StockManipulator { get; }

        public HttpTriggers(IProductsDictionary products,
                            IShoppingCartFactory shoppingCartFactory,
                            IShoppingCartManipulator shoppingCartManipulator,
                            IStockManipulator stockManipulator)
        {
            Products = products ?? throw new ArgumentNullException(nameof(products));
            ShoppingCartFactory = shoppingCartFactory ?? throw new ArgumentNullException(nameof(shoppingCartFactory));
            CartManipulator = shoppingCartManipulator ?? throw new ArgumentNullException(nameof(shoppingCartManipulator));
            StockManipulator = stockManipulator ?? throw new ArgumentNullException(nameof(stockManipulator));
        }

        [FunctionName("products")]
        public IActionResult RunGetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed GET products.");

            return
                PerformAuthenticatedFunc(req, (_) =>
                {
                    List<Product> response = Products.GetAll();

                    return new OkObjectResult(response);
                });
        }

        [FunctionName("add-payment")]
        public IActionResult RunAddPayment(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "add-payment")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed POST add-payment.");

            return
                PerformAuthenticatedFunc(req, (cart) => 
                {
                    if (!decimal.TryParse(req.Query["money"], out decimal money))
                    {
                        return new BadRequestObjectResult("Query Param: \"money\" must be parsable as a decimal");
                    }

                    CartManipulator.AddPayment(cart, money);

                    return new OkObjectResult("");
                });
        }

        [FunctionName("purchase")]
        public IActionResult RunPurchase(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "purchase")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed POST purchase.");

            return
                PerformAuthenticatedFunc(req, (cart) => 
                {
                    var item = req.Query["item"];
                    if (string.IsNullOrEmpty(item))
                    {
                        return new BadRequestObjectResult("Query Param: \"item\" must be specified.");
                    }

                    Product product = Products.GetItem(item);
                    if (StockManipulator.IsValidPurchaseRequest(product, cart.RemainingFunds))
                    {
                        StockManipulator.ReduceStockLevel(product);
                        CartManipulator.AddItem(product, cart);
                    }
                    else
                    {
                        return new BadRequestObjectResult("Unable to purchase item");
                    }

                    return new OkResult();
                });


        }

        [FunctionName("final-purchase")]
        public IActionResult RunFinalPurchase(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "final-purchase")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed GET final-purchase.");

            return 
                PerformAuthenticatedFunc(req, (cart) =>
                {
                    var result = new OkObjectResult(cart);

                    ShoppingCartFactory.RemoveCart(cart.UserId);

                    return result;
                });
        }

        private IActionResult PerformAuthenticatedFunc(HttpRequest req, Func<ShoppingCart, IActionResult> delegateFunc)
        {
            try
            {
                if (!int.TryParse(req.Headers["x-id"], out int userId))
                {
                    throw new UnauthorizedAccessException();
                }
                ShoppingCart cart = ShoppingCartFactory.AddOrRetrieveCart(userId);

                return delegateFunc(cart);
            }
            catch (UnauthorizedAccessException)
            {
                return new UnauthorizedResult();
            }
        }
    }
}
