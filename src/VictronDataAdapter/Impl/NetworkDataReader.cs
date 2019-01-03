﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    internal class NetworkDataReader : IDataReader
    {
        private readonly NetworkStream stream;

        public NetworkDataReader(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void Dispose()
        {
            this.stream.Dispose();
        }

        public async Task<bool> WaitForAvailable(int timeout = Timeout.Infinite)
        {
            var start = DateTime.UtcNow;
            do
            {
                if (this.stream.DataAvailable)
                    return true;
                await Task.Delay(100);
            } while (timeout == Timeout.Infinite || (DateTime.UtcNow - start).TotalMilliseconds < timeout);
            return false;
        }

        public async Task<byte[]> ReadAvailable()
        {
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
