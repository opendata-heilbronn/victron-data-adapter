using VeDirectCommunication.Parser;

namespace VeDirectCommunication.HexMode
{
    public class VictronHexMessage : IVictronMessage
    {
        public byte[] Nibbles { get; set; }

        public VictronMessageType MessageType => VictronMessageType.Hex;
    }
}
