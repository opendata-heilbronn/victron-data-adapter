using System;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Tests
{
    class MockDataReader : IDataReader
    {
        private int line;
        private readonly string[] data;

        public MockDataReader(string data)
        {
            this.data = data.Split(Environment.NewLine);
            this.line = 0;
        }

        public void Dispose()
        {
        }

        public Task<string> ReadLine(int timeout = -1)
        {
            return Task.FromResult(line >= this.data.Length ? null : this.data[line++]);
        }
    }
}
