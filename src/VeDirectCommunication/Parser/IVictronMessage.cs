namespace VeDirectCommunication.Parser
{
    public interface IVictronMessage
    {
        VictronMessageType MessageType { get; }
    }
}
