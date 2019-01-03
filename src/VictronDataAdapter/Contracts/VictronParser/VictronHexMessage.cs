using System.Collections.Generic;

namespace VictronDataAdapter.Contracts.VictronParser
{
    public class VictronHexMessage : IVictronMessage
    {
        public IList<byte> Bytes { get; set; }

        public VictronMessageType MessageType => VictronMessageType.Hex;
    }
}
