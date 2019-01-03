using System;
using System.Threading;
using System.Threading.Tasks;

namespace VictronDataAdapter.Contracts
{
    public interface IDataReader : IDisposable
    {
        Task<bool> WaitForAvailable(int timeout = Timeout.Infinite);
        Task<byte[]> ReadAvailable();
    }
}
