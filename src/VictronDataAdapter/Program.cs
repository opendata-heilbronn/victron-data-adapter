using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Impl;

namespace VictronDataAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            new HostBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureAppConfiguration(x =>
                {
                    x.AddJsonFile("appSettings.json");
                    x.AddEnvironmentVariables();
                })
                .RunConsoleAsync()
                .Wait();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IVictronMessageParser, VictronMessageParser>();
            services.AddSingleton<IVictronStreamAdapter, VictronStreamAdapter>();

            services.AddSingleton<IVictronDataSource, VictronIpDataSource>();
            services.Configure<IpDataSourceConfig>(context.Configuration.GetSection("IpDataSource"));

            services.AddHostedService<Host>();
        }
    }
}
