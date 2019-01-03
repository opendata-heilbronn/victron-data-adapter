using System.Collections.Generic;

namespace VictronDataAdapter.Contracts.VictronParser
{
    public class VictronTextBlock : IVictronMessage
    {
        public bool ChecksumValid { get; set; }
        public IList<VictronTextMessage> Messages { get; set; }

        public VictronMessageType MessageType => VictronMessageType.Text;
    }
}
