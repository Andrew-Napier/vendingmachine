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
        public IHttpRequestHandler HttpRequestHandler { get; }
        public IProductsDictionary Products { get; }
        public IShoppingCartFactory ShoppingCartFactory { get; }
        public IShoppingCartManipulator CartManipulator { get; }
        public IStockManipulator StockManipulator { get; }

        public HttpTriggers(IHttpRequestHandler httpRequestHandler,
                            IProductsDictionary products,
                            IShoppingCartFactory shoppingCartFactory,
                            IShoppingCartManipulator shoppingCartManipulator,
                            IStockManipulator stockManipulator)
        {
            HttpRequestHandler = httpRequestHandler ?? throw new ArgumentNullException(nameof(httpRequestHandler));
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
            HttpRequestHandler.InitialiseForRequest(req);

            return
                HttpRequestHandler.PerformAuthenticatedFunc((c, _) =>
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

            HttpRequestHandler.InitialiseForRequest(req);
            HttpRequestHandler.AddMandatoryQueryParameter("money", typeof(decimal));

            return
                HttpRequestHandler.PerformAuthenticatedFunc((cart, parameters) => 
                {
                    var money = (decimal)parameters["money"];

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
            HttpRequestHandler.InitialiseForRequest(req);
            HttpRequestHandler.AddMandatoryQueryParameter("item", typeof(string));

            return
                HttpRequestHandler.PerformAuthenticatedFunc((cart, parameters) => 
                {
                    var item = (string)parameters["item"];
                    Product product = Products.GetItem(item);

                    if (!StockManipulator.IsValidPurchaseRequest(product, cart.RemainingFunds))
                    {
                        return new BadRequestObjectResult("Unable to purchase item");
                    }

                    StockManipulator.ReduceStockLevel(product);
                    CartManipulator.AddItem(product, cart);

                    return new OkResult();
                });


        }

        [FunctionName("final-purchase")]
        public IActionResult RunFinalPurchase(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "final-purchase")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed GET final-purchase.");

            HttpRequestHandler.InitialiseForRequest(req);
            return
                HttpRequestHandler.PerformAuthenticatedFunc((cart, _) =>
                {
                    var result = new OkObjectResult(cart);

                    ShoppingCartFactory.RemoveCart(cart.UserId);

                    return result;
                });
        }
    }
}
