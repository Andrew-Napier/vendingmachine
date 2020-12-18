using Moq;
using NUnit.Framework;
using VendingMachine;
using VendingMachine.Models;
using VendingMachine.Services;

namespace VendingMachineTests
{
    public class StockManipulatorTests
    {
        [Test]
        public void TestPerfectPathPurchase()
        {
            // Given  
            var mockProducts = new Mock<IProductsDictionary>();
            var mockProduct = new Product { Id = "a", Price = 1.00M, Quantity = 1 };

            var sut = new StockManipulator(mockProducts.Object);
            // When

            var actual = sut.IsValidPurchaseRequest(mockProduct, 1.00M);

            // Then
            Assert.That(actual, Is.True);
        }

        [TestCase(10.00, 1, 5.00)]
        [TestCase(1.00, 0, 5.00)]
        [TestCase(-1.0, 10, 1.00)]
        [TestCase(1.0, 1, -2.00)]
        public void TestInvalidPurchaseScenarios(decimal price, int qty, decimal funds)
        {
            var mockProducts = new Mock<IProductsDictionary>();
            var mockProduct = new Product { Id = "a", Price = price, Quantity = qty };

            var sut = new StockManipulator(mockProducts.Object);
            // When

            var actual = sut.IsValidPurchaseRequest(mockProduct, funds);
            // Then
            Assert.That(actual, Is.False);
        }

        [Test]
        public void TestPurchase()
        {
            // Given  
            var mockProducts = new Mock<IProductsDictionary>();
            var mockProduct = new Product { Id = "a", Price = 1.00M, Quantity = 1 };
            mockProducts.Setup(x => x.GetItem("a")).Returns(mockProduct);

            var sut = new StockManipulator(mockProducts.Object);
            // When
            sut.ReduceStockLevel(mockProduct);

            // Then
            mockProducts.Verify(x => x.Update(It.IsAny<Product>()), Times.Once());
        }
    }
}