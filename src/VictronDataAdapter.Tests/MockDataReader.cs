using System;
using System.IO;
using System.Threading.Tasks;
using VeDirectCommunication;

namespace VictronDataAdapter.Tests
{
    class MockDataReader : IVictronStream
    {
        private readonly MemoryStream _data;

        public MockDataReader(byte[] data)
        {
            _data = new MemoryStream(data);
            _data.Position = 0;
        }

        public void Dispose()
        {
        }

        public Task<byte[]> ReadAvailable()
        {
            var buffer = new byte[5];
            var length = _data.Read(buffer, 0, 5);

            var toReturn = new byte[length];
            Array.Copy(buffer, toReturn, length);

            return Task.FromResult(toReturn);
        }

        public Task Write(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DataAvailable()
        {
            return Task.FromResult(_data.Position != _data.Length);
        }
    }
}
