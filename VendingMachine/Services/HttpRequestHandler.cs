using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using VendingMachine.Models;

namespace VendingMachine.Services
{
    public interface IHttpRequestHandler    
    {
        IActionResult PerformAuthenticatedFunc(Func<ShoppingCart, Dictionary<string, object>, IActionResult> delegateFunc);
        void InitialiseForRequest(HttpRequestMessage req);
        void AddMandatoryQueryParameter(string paramName, Type type);
    }

    public class HttpRequestHandler : IHttpRequestHandler
    {
        public IShoppingCartFactory ShoppingCartFactory { get; }
        private HttpRequestMessage request;
        private readonly Dictionary<string, Type> mandatoryValues = new Dictionary<string, Type>();

        public HttpRequestHandler(IShoppingCartFactory shoppingCartFactory)
        {
            ShoppingCartFactory = shoppingCartFactory ?? throw new ArgumentNullException(nameof(shoppingCartFactory));
        }

        public IActionResult PerformAuthenticatedFunc(
            Func<ShoppingCart, Dictionary<string, object>, IActionResult> delegateFunc)
        {
            var parameters = new Dictionary<string, object>();
            string id = request.Headers.TryGetValues("x-id", out IEnumerable<string> values) 
                ? values.FirstOrDefault()
                : null;

            if (!int.TryParse(id, out int userId))
            {
                return new UnauthorizedResult();
            }

            var queryCollection = request.RequestUri?.ParseQueryString() ?? new NameValueCollection();

            foreach (var paramDetail in mandatoryValues)
            {
                var item = queryCollection.Get(paramDetail.Key);
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

        public void InitialiseForRequest(HttpRequestMessage req)
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
