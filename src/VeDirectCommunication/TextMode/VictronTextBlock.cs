using System.Collections.Generic;
using VeDirectCommunication.Parser;

namespace VeDirectCommunication.TextMode
{
    public class VictronTextBlock : IVictronMessage
    {
        public bool ChecksumValid { get; set; }
        public IList<VictronTextField> Fields { get; set; }

        public VictronMessageType MessageType => VictronMessageType.Text;
    }
}
