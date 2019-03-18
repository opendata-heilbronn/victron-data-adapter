using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using VeDirectCommunication;

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
            services.AddSingleton<IVictronStreamAdapter, VictronStreamAdapter>();
            
            services.Configure<IpDataSourceConfig>(context.Configuration.GetSection("IpDataSource"));
            services.UseVeDirectCommunication<NetworkVictronStream>();
            
            services.Configure<InfluxDbConfiguration>(context.Configuration.GetSection("InfluxDb"));
            services.AddHostedService<Host>();
        }
    }
}
