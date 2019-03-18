using System;
using System.Threading.Tasks;

namespace VeDirectCommunication
{
    public interface IVictronStream : IDisposable
    {
        Task Write(byte[] bytes);
        Task<bool> DataAvailable();
        Task<byte[]> ReadAvailable();
    }
}
