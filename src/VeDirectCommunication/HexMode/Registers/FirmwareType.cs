namespace VeDirectCommunication.HexMode.Registers
{
    public enum FirmwareType
    {
        Bootloader = 0x00,
        Application = 0x01,
        Tester = 0x02,
        ReleaseCandidate = 0x03
    }
}
