using Microsoft.Extensions.Options;
using System.Net.Sockets;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    class VictronIpDataSource : IVictronDataSource
    {
        private readonly IpDataSourceConfig _config;
        private TcpClient _client;

        public VictronIpDataSource(IOptions<IpDataSourceConfig> config)
        {
            _config = config.Value;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        public IDataReader GetDataReader()
        {
            _client = new TcpClient();
            _client.ReceiveTimeout = 100;
            _client.Connect(_config.Hostname, _config.Port.Value);

            return new NetworkDataReader(_client.GetStream());
        }
    }
}
