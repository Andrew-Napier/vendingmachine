using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(VendingMachine.Services.Startup))]
namespace VendingMachine.Services
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .InitialiseProductsDictionary()
                .AddTransient<IHttpRequestHandler, HttpRequestHandler>()
                .AddSingleton<IShoppingCartFactory, ShoppingCartFactory>()
                .AddTransient<IShoppingCartManipulator, ShoppingCartManipulator>()
                .AddTransient<IStockManipulator, StockManipulator>()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder.SetMinimumLevel(LogLevel.Debug);
                });
        }
    }
}
