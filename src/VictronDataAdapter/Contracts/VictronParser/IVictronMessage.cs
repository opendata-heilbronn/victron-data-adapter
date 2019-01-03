namespace VictronDataAdapter.Contracts.VictronParser
{
    public interface IVictronMessage
    {
        VictronMessageType MessageType { get; }
    }
}
