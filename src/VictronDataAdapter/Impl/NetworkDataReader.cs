using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    internal class NetworkDataReader : IDataReader
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private readonly IpDataSourceConfig config;

        public NetworkDataReader(IpDataSourceConfig config)
        {
            this.config = config;
            this.tcpClient = new TcpClient();
        }

        private async Task Connect()
        {
            this.stream?.Dispose();
            this.tcpClient?.Dispose();

            this.tcpClient = new TcpClient();
            this.tcpClient.SendTimeout = 1000;

            await this.tcpClient.ConnectAsync(this.config.Hostname, this.config.Port.Value);
            this.stream = this.tcpClient.GetStream();
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public async Task<bool> WaitForAvailable(int timeout = Timeout.Infinite)
        {
            int loopDelay = 100;
            int i = 0;

            var start = DateTime.UtcNow;
            do
            {
                if (!this.tcpClient.Connected)
                {
                    await Connect();
                }

                if (this.stream.DataAvailable)
                    return true;
                await Task.Delay(loopDelay);

                i++;

                if ((i * loopDelay) % (1000 * 60) == 0) // if no response in 10 seconds, send bogus data to check if connection still alive
                {
                    var cmd = ":352\n"; // get firmware version cmd as to not confuse the charge controller if rx is connected
                    await this.stream.WriteAsync(Encoding.ASCII.GetBytes(cmd));
                }
            } while (timeout == Timeout.Infinite || (DateTime.UtcNow - start).TotalMilliseconds < timeout);
            return false;
        }

        public async Task<byte[]> ReadAvailable()
        {
            if (!this.tcpClient.Connected)
            {
                await Connect();
            }

            byte[] buf = new byte[1024];
            using (var memoryStream = new MemoryStream())
            {
                while (this.stream.DataAvailable)
                {
                    var readBytes = await this.stream.ReadAsync(buf, 0, 1024);
                    memoryStream.Write(buf, 0, readBytes);
                }

                return memoryStream.ToArray();
            }
        }
    }
}
