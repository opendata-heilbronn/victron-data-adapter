using System.Collections.Generic;
using VeDirectCommunication.TextMode;

namespace VeDirectCommunication.Parser
{
    internal class VictronParserState
    {
        public VictronParserState()
        {
            ParseState = ParseState.Idle;
            Checksum = 0;
            RecordName = "";
            RecordValue = "";
            Records = new List<VictronTextField>();
            HexRecordNibbles = new List<byte>();
        }

        public ParseState ParseState { get; set; }
        public byte Checksum { get; set; }
        public string RecordName { get; set; }
        public string RecordValue { get; set; }
        public IList<VictronTextField> Records { get; set; }
        public IList<byte> HexRecordNibbles { get; set; }
        public bool LowNibbleSet { get; set; }
        public byte LowNibble { get; set; }
    }
}