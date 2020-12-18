using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(VendingMachine.Services.Startup))]
namespace VendingMachine.Services
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .InitialiseProductsDictionary()
                .AddSingleton<IShoppingCartFactory, ShoppingCartFactory>()
                .AddTransient<IShoppingCartManipulator, ShoppingCartManipulator>()
                .AddTransient<IStockManipulator, StockManipulator>();
        }
    }
}
