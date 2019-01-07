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
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly IpDataSourceConfig _config;

        public NetworkDataReader(IpDataSourceConfig config)
        {
            _config = config;
            _tcpClient = new TcpClient();
        }

        private async Task Connect()
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();

            _tcpClient = new TcpClient();
            _tcpClient.SendTimeout = 1000;

            await _tcpClient.ConnectAsync(_config.Hostname, _config.Port.Value);
            _stream = _tcpClient.GetStream();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public async Task<bool> WaitForAvailable(int timeout = Timeout.Infinite)
        {
            int loopDelay = 100;
            int i = 0;

            var start = DateTime.UtcNow;
            do
            {
                if (!_tcpClient.Connected)
                {
                    await Connect();
                }

                if (_stream.DataAvailable)
                    return true;
                await Task.Delay(loopDelay);

                i++;

                if ((i * loopDelay) % (1000 * 60) == 0) // if no response in 10 seconds, send bogus data to check if connection still alive
                {
                    var cmd = ":352\n"; // get firmware version cmd as to not confuse the charge controller if rx is connected
                    await _stream.WriteAsync(Encoding.ASCII.GetBytes(cmd));
                }
            } while (timeout == Timeout.Infinite || (DateTime.UtcNow - start).TotalMilliseconds < timeout);
            return false;
        }

        public async Task<byte[]> ReadAvailable()
        {
            if (!_tcpClient.Connected)
            {
                await Connect();
            }

            byte[] buf = new byte[1024];
            using (var memoryStream = new MemoryStream())
            {
                while (_stream.DataAvailable)
                {
                    var readBytes = await _stream.ReadAsync(buf, 0, 1024);
                    memoryStream.Write(buf, 0, readBytes);
                }

                return memoryStream.ToArray();
            }
        }
    }
}
