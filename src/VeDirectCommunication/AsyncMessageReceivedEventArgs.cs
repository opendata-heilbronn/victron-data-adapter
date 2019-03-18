using VeDirectCommunication.HexMode.HexMessages;

namespace VeDirectCommunication
{
    public class AsyncMessageReceivedEventArgs
    {
        public AsyncMessageReceivedEventArgs(AsyncRegisterResponseMessage data)
        {
            Data = data;
        }
        public AsyncRegisterResponseMessage Data { get; }
    }
}