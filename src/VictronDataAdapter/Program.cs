using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Impl;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts.VictronParser;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

namespace VictronDataAdapter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                })
                .ConfigureServices(ConfigureServices)
                .ConfigureAppConfiguration((ctx, x) =>
                {
                    x.AddJsonFile("appSettings.json", optional: true);
                    x.AddJsonFile($"appSettings.user.{Environment.UserName}.json", optional: true);
                    x.AddEnvironmentVariables();
                }).Build();

            var hasErrors = false;
            foreach (var error in host.Services.ValidateConfig<IpDataSourceConfig>())
            {
                hasErrors = true;
                Console.WriteLine($"Error in Section IpDataSource: {error}");
            }

            foreach (var error in host.Services.ValidateConfig<InfluxDbConfiguration>())
            {
                hasErrors = true;
                Console.WriteLine($"Error in Section InfluxDb: {error}");
            }

            if (hasErrors)
            {
                Environment.Exit(1);
                return;
            }

            host.Run();
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
