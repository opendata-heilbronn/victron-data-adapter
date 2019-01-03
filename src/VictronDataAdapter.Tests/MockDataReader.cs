﻿using System;
using System.IO;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Tests
{
    class MockDataReader : IDataReader
    {
        private readonly MemoryStream data;

        public MockDataReader(byte[] data)
        {
            this.data = new MemoryStream(data);
            this.data.Position = 0;
        }

        public void Dispose()
        {
        }

        public Task<bool> WaitForAvailable(int timeout = -1)
        {
            return Task.FromResult(this.data.Position == this.data.Length);
        }

        public Task<byte[]> ReadAvailable()
        {
            var buffer = new byte[5];
            var length = this.data.Read(buffer, 0, 5);

            var toReturn = new byte[length];
            Array.Copy(buffer, toReturn, length);

            return Task.FromResult(toReturn);
        }
    }
}
