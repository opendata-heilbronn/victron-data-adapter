namespace VeDirectCommunication.Parser
{
    internal enum ParseState
    {
        Checksum,
        HexRecord,
        Idle,
        RecordBegin,
        RecordName,
        RecordValue
    }
}