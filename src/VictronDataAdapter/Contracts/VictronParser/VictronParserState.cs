using System.Collections.Generic;

namespace VictronDataAdapter.Contracts.VictronParser
{
    public class VictronParserState
    {
        public VictronParserState()
        {
            ParseState = ParseState.Idle;
            Checksum = 0;
            RecordName = "";
            RecordValue = "";
            Records = new List<VictronTextMessage>();
            HexRecordBytes = new List<byte>();
        }

        public ParseState ParseState { get; set; }
        public byte Checksum { get; set; }
        public string RecordName { get; set; }
        public string RecordValue { get; set; }
        public IList<VictronTextMessage> Records { get; set; }
        public IList<byte> HexRecordBytes { get; set; }
        public bool LowNibbleSet { get; set; }
        public byte LowNibble { get; set; }
    }
}