using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Sockets;
using System.Text;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    class VictronIpDataSource : IVictronDataSource
    {
        private readonly IpDataSourceConfig config;
        private TcpClient client;

        public VictronIpDataSource(IOptions<IpDataSourceConfig> config)
        {
            this.config = config.Value;
        }

        public void Dispose()
        {
            this.client?.Dispose();
        }

        public IDataReader GetDataReader()
        {
            this.client = new TcpClient();
            this.client.ReceiveTimeout = 100;
            this.client.Connect(this.config.Hostname, this.config.Port);

            return new NetworkDataReader(this.client.GetStream());
        }
    }
}
