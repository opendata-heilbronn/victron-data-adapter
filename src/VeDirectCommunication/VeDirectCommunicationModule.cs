using Microsoft.Extensions.DependencyInjection;
using VeDirectCommunication.HexMode;
using VeDirectCommunication.Parser;

namespace VeDirectCommunication
{
    public static class VeDirectCommunicationModule
    {
        public static void UseVeDirectCommunication<TVictronStream>(this IServiceCollection services)
            where TVictronStream : class, IVictronStream
        {
            services.AddSingleton<IVictronParser, VictronParser>();
            services.AddSingleton<IVictronHexMessageSerializer, VictronHexMessageSerializer>();
            services.AddTransient<IVictronStream, TVictronStream>();
            services.AddTransient<IVeDirectDevice, VeDirectDevice>();
        }
    }
}
