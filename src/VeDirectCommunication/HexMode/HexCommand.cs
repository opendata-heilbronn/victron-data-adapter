namespace VeDirectCommunication.HexMode
{
    public enum HexCommand : byte
    {
        EnterBoot = 0x0,
        Ping = 0x1,
        AppVersion = 0x3,
        DeviceId = 0x4,
        Restart = 0x6,
        Get = 0x7,
        Set = 0x8
    }
}
