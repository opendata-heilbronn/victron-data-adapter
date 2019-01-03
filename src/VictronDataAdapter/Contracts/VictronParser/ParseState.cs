namespace VictronDataAdapter.Contracts.VictronParser
{
    public enum ParseState
    {
        Checksum,
        HexRecord,
        Idle,
        RecordBegin,
        RecordName,
        RecordValue
    }
}