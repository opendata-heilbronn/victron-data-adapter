namespace VeDirectCommunication.HexMode.HexMessages
{
    public abstract class RegisterValueResponseMessage : IHexResponseMessage
    {
        public abstract HexResponse ResponseType { get; }
        public VictronRegister Register { get; set; }
        public GetSetResponseFlags Flags { get; set; }
        public byte[] RegisterValue { get; set; }
    }
}
