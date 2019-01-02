using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    class NetworkDataReader : IDataReader
    {
        private readonly NetworkStream stream;
        private string currentLine = "";

        public NetworkDataReader(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public async Task<string> ReadLine(int timeout = Timeout.Infinite)
        {
            byte[] buf = new byte[1024];
            var start = DateTime.UtcNow;
            do
            {
                while (this.stream.DataAvailable)
                {
                    var readBytes = await this.stream.ReadAsync(buf, 0, 1024);
                    this.currentLine += Encoding.ASCII.GetString(buf, 0, readBytes); // this won't work with multi-byte encodings
                }

                var endIdx = this.currentLine.IndexOf('\n');
                if(endIdx != -1)
                {
                    var toReturn = this.currentLine.Substring(0, endIdx);
                    this.currentLine = this.currentLine.Substring(endIdx + 1);
                    return toReturn;
                }
                await Task.Delay(100);
            } while (timeout == Timeout.Infinite || (DateTime.UtcNow - start).TotalMilliseconds < timeout);

            return null;
        }
    }
}
