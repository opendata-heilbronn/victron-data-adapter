using System;
using System.Threading.Tasks;
using VeDirectCommunication.HexMode;
using VeDirectCommunication.HexMode.Registers;

namespace VeDirectCommunication
{
    public interface IVeDirectDevice
    {
        event EventHandler<TextMessageReceivedEventArgs> TextMessageReceived;
        event EventHandler<AsyncMessageReceivedEventArgs> AsyncMessageReceived;

        void Dispose();
        Task<byte[]> GetRegister(VictronRegister register);
        Task SetRegister(VictronRegister register, byte[] data);
        Task<VictronVersion> Ping();
        Task Start();
        Task Stop();
    }
}