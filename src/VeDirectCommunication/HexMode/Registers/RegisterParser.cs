using System;
using System.Text;

namespace VeDirectCommunication.HexMode.Registers
{
    public class RegisterParser
    {
        public VictronVersion ParsePingResponse(byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("Invalid number of bytes");
            }

            var type = (byte)((bytes[1] & 0xF0) >> 6);
            var majorVersion = (bytes[1] & 0x0F);
            var minorVersion = ((bytes[0] & 0xF0) >> 4) * 10 + (bytes[0] & 0x0F);

            //TODO: release candidate number

            return new VictronVersion
            {
                FirmwareType = (FirmwareType)type,
                VersionNumber = new Version(majorVersion, minorVersion)
            };
        }

        public ushort ParseUInt16(byte[] bytes)
        {
            return (ushort)(bytes[0] | bytes[1] << 8);
        }

        public short ParseSInt16(byte[] bytes)
        {
            return (short)(bytes[0] | bytes[1] << 8);
        }

        public string ParseString(byte[] bytes)
        {
            if(bytes.Length < 1)
            {
                throw new ArgumentException("Invalid String");
            }

            return Encoding.ASCII.GetString(bytes, 0, Array.FindIndex(bytes, x => x == 0x00) - 1);
        }

        public uint ParseUInt32(byte[] bytes)
        {
            return (uint)(bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24);
        }
    }
}
