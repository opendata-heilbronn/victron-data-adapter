using Microsoft.Extensions.Options;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    class VictronIpDataReaderFactory : IVictronDataReaderFactory
    {
        private readonly IpDataSourceConfig _config;

        public VictronIpDataReaderFactory(IOptions<IpDataSourceConfig> config)
        {
            _config = config.Value;
        }

        public IDataReader GetDataReader()
        {
            return new NetworkDataReader(_config);
        }
    }
}
