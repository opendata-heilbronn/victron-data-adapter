using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VeDirectCommunication
{
    public class NetworkVictronStream : IVictronStream
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly IpDataSourceConfig _config;
        private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1);

        public NetworkVictronStream(IOptions<IpDataSourceConfig> config)
        {
            _config = config.Value;
            _tcpClient = new TcpClient();
        }

        private async Task EnsureConnected()
        {
            await _connectSemaphore.WaitAsync();
            try
            {
                if (_tcpClient.Connected)
                    return;

                _stream?.Dispose();
                _tcpClient?.Dispose();

                _tcpClient = new TcpClient();
                _tcpClient.SendTimeout = 1000;

                await _tcpClient.ConnectAsync(_config.Hostname, _config.Port.Value);
                _stream = _tcpClient.GetStream();
            }
            finally
            {
                _connectSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public async Task<byte[]> ReadAvailable()
        {
            await EnsureConnected();

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

        public async Task Write(byte[] bytes)
        {
            await EnsureConnected();
            await _stream.WriteAsync(bytes, 0, bytes.Length);
        }

        public async Task<bool> DataAvailable()
        {
            await EnsureConnected();
            return _stream.DataAvailable;
        }
    }
}
