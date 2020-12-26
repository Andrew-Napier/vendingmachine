using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using VendingMachine.Models;

namespace VendingMachine.Services
{
    public interface IHttpRequestHandler    
    {
        IActionResult PerformAuthenticatedFunc(Func<ShoppingCart, Dictionary<string, object>, IActionResult> delegateFunc);
        void InitialiseForRequest(HttpRequest req);
        void AddMandatoryQueryParameter(string paramName, Type type);
    }

    public class HttpRequestHandler : IHttpRequestHandler
    {
        public IShoppingCartFactory ShoppingCartFactory { get; }
        private HttpRequest request;
        private readonly Dictionary<string, Type> mandatoryValues = new Dictionary<string, Type>();

        public HttpRequestHandler(IShoppingCartFactory shoppingCartFactory)
        {
            ShoppingCartFactory = shoppingCartFactory ?? throw new ArgumentNullException(nameof(shoppingCartFactory));
        }

        public IActionResult PerformAuthenticatedFunc(
            Func<ShoppingCart, Dictionary<string, object>, IActionResult> delegateFunc)
        {
            var parameters = new Dictionary<string, object>();

            if (!int.TryParse(request.Headers["x-id"], out int userId))
            {
                return new UnauthorizedResult();
            }

            foreach (var paramDetail in mandatoryValues)
            {
                var item = request.Query[paramDetail.Key].FirstOrDefault();
                if (string.IsNullOrEmpty(item))
                {
                    return new BadRequestObjectResult($"Query Param: \"{paramDetail.Key}\" must be specified.");
                }
                object paramValue = Convert.ChangeType(item, paramDetail.Value);
                if (paramValue is null)
                {
                    return new BadRequestObjectResult($"Query Param: \"{paramDetail.Key}\" is incorrectly formatted.");
                }

                parameters.Add(paramDetail.Key, paramValue);
            }

            ShoppingCart cart = ShoppingCartFactory.AddOrRetrieveCart(userId);

            return delegateFunc(cart, parameters);
        }

        public void InitialiseForRequest(HttpRequest req)
        {
            request = req;
            mandatoryValues.Clear();
        }

        public void AddMandatoryQueryParameter(string paramName, Type type)
        {
            mandatoryValues.Add(paramName, type);
        }
    }
}
