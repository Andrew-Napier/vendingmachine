using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Net.Http;
using VendingMachine.Services;

namespace VendingMachineTests
{
    public class HttpRequestHandlerTests
    {
        [Test]
        public void TestMandatoryParameterExists()
        {
            // Given  
            var mockShoppingCartFactory = new Mock<IShoppingCartFactory>();
            var sut = new HttpRequestHandler(mockShoppingCartFactory.Object);

            HttpRequestMessage mockRequest = new HttpRequestMessage();
            mockRequest.RequestUri = new Uri("http://myvendingmachine.com/api/purchase/?item=handcream");
            mockRequest.Headers.Add("x-id", "42");

            // When
            sut.InitialiseForRequest(mockRequest);
            sut.AddMandatoryQueryParameter("item", typeof(string));

            // Then
            sut.PerformAuthenticatedFunc((cart, parameters) =>
            {
                Assert.That(parameters.ContainsKey("item"));
                return new OkResult();
            });
        }

        [Test]
        public void TestMissingHeaderParamReturns401()
        {
            // Given  
            var mockShoppingCartFactory = new Mock<IShoppingCartFactory>();
            var sut = new HttpRequestHandler(mockShoppingCartFactory.Object);

            HttpRequestMessage mockRequest = new HttpRequestMessage();
            mockRequest.RequestUri = new Uri("http://myvendingmachine.com/api/purchase/?item=handcream");

            // When
            sut.InitialiseForRequest(mockRequest);
            var actual = sut.PerformAuthenticatedFunc((c, p) => { return new OkResult(); });

            // Then
            Assert.That(actual, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public void TestMissingMandatoryParamReturns400()
        {
            // Given  
            var mockShoppingCartFactory = new Mock<IShoppingCartFactory>();
            var sut = new HttpRequestHandler(mockShoppingCartFactory.Object);

            HttpRequestMessage mockRequest = new HttpRequestMessage();
            mockRequest.RequestUri = new Uri("http://myvendingmachine.com/api/purchase/?product=handcream");
            mockRequest.Headers.Add("x-id", "42");

            // When
            sut.InitialiseForRequest(mockRequest);
            sut.AddMandatoryQueryParameter("item", typeof(string));
            var actual = sut.PerformAuthenticatedFunc((c, p) => { return new OkResult(); });

            // Then
            Assert.That(actual, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public void TestParsingQueryParamErrorReturns400()
        {
            var mockShoppingCartFactory = new Mock<IShoppingCartFactory>();
            var sut = new HttpRequestHandler(mockShoppingCartFactory.Object);

            HttpRequestMessage mockRequest = new HttpRequestMessage();
            mockRequest.RequestUri = new Uri("http://myvendingmachine.com/api/add-payment/?money=asdf");
            mockRequest.Headers.Add("x-id", "42");

            // When
            sut.InitialiseForRequest(mockRequest);
            sut.AddMandatoryQueryParameter("money", typeof(decimal));
            var actual = sut.PerformAuthenticatedFunc((c, p) => { return new OkResult(); });

            // Then
            Assert.That(actual, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public void TestBlankQueryParamErrorReturns400()
        {
            var mockShoppingCartFactory = new Mock<IShoppingCartFactory>();
            var sut = new HttpRequestHandler(mockShoppingCartFactory.Object);

            HttpRequestMessage mockRequest = new HttpRequestMessage();
            mockRequest.RequestUri = new Uri("http://myvendingmachine.com/api/purchase/?item=");
            mockRequest.Headers.Add("x-id", "42");

            // When
            sut.InitialiseForRequest(mockRequest);
            sut.AddMandatoryQueryParameter("item", typeof(string));
            var actual = sut.PerformAuthenticatedFunc((c, p) => { return new OkResult(); });

            // Then
            Assert.That(actual, Is.TypeOf<BadRequestObjectResult>());
        }
    }
}
