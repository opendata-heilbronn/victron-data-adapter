namespace VictronDataAdapter.Contracts
{
    public interface IVictronMessageParser
    {
        VictronMessage ParseLine(string line);
    }
}
