using Microsoft.Extensions.Options;
using System.Net.Sockets;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    class VictronIpDataReaderFactory : IVictronDataReaderFactory
    {
        private readonly IpDataSourceConfig config;
        private TcpClient client;

        public VictronIpDataReaderFactory(IOptions<IpDataSourceConfig> config)
        {
            this.config = config.Value;
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public IDataReader GetDataReader()
        {
            return new NetworkDataReader(this.config);
        }
    }
}
