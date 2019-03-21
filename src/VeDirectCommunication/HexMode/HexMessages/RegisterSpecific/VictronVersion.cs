using System;

namespace VeDirectCommunication.HexMode.HexMessages.RegisterSpecific
{
    public class VictronVersion
    {
        public Version VersionNumber { get; set; }
        public FirmwareType FirmwareType { get; set; }
    }
}
