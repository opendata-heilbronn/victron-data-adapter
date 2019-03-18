using System.Threading.Tasks;
using VeDirectCommunication.HexMode;

namespace VeDirectCommunication
{
    public interface IVeDirectDevice
    {
        event TextMessageReceivedHandler TextMessageReceived;
        event AsyncMessageReceivedHandler AsyncMessageReceived;

        void Dispose();
        Task<byte[]> GetRegister(VictronRegister register);
        Task SetRegister(VictronRegister register, byte[] data);
        Task<byte[]> Ping();
        Task Start();
        Task Stop();
    }
}