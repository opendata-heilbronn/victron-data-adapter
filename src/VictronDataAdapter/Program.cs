using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Impl;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts.VictronParser;
using System;

namespace VictronDataAdapter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Task.Run(() => new HostBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureAppConfiguration((ctx, x) =>
                {
                    x.AddJsonFile("appSettings.json");
                    x.AddJsonFile($"appSettings.user.{Environment.UserName}.json", optional: true);
                    x.AddEnvironmentVariables();
                })
                .RunConsoleAsync())
                .Wait();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IVictronParser, VictronParser>();
            services.AddSingleton<IVictronStreamAdapter, VictronStreamAdapter>();

            services.AddSingleton<IVictronDataSource, VictronIpDataSource>();
            services.Configure<IpDataSourceConfig>(context.Configuration.GetSection("IpDataSource"));

            services.Configure<InfluxDbConfiguration>(context.Configuration.GetSection("InfluxDb"));
            services.AddHostedService<Host>();
        }
    }
}
