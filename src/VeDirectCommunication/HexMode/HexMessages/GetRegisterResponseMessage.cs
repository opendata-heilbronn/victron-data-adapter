namespace VeDirectCommunication.HexMode.HexMessages
{
    public class GetRegisterResponseMessage : RegisterValueResponseMessage
    {
        public override HexResponse ResponseType => HexResponse.Get;
    }
}
