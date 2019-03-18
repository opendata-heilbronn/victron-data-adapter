namespace VeDirectCommunication.HexMode
{
    public enum HexResponse : byte
    {
        Done = 0x1,
        Unknown = 0x3,
        Error = 0x4,
        Ping = 0x5,
        Get = 0x7,
        Set = 0x8,
        Async = 0xA
    }
}
