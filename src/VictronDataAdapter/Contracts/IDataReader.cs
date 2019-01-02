using System;
using System.Threading;
using System.Threading.Tasks;

namespace VictronDataAdapter.Contracts
{
    public interface IDataReader : IDisposable
    {
        Task<string> ReadLine(int timeout = Timeout.Infinite);
    }
}
