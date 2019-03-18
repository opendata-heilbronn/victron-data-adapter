namespace VeDirectCommunication.HexMode.HexMessages
{
    public class PingResponseMessage : IHexResponseMessage
    {
        public HexResponse ResponseType => HexResponse.Ping;
        public byte[] Version { get; set; }
    }
}
