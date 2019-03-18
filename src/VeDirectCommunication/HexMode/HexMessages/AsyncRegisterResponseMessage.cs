namespace VeDirectCommunication.HexMode.HexMessages
{
    public class AsyncRegisterResponseMessage : RegisterValueResponseMessage
    {
        public override HexResponse ResponseType => HexResponse.Async;
    }
}
