using System.Collections.Generic;

namespace VictronDataAdapter.Contracts.VictronParser
{
    public interface IVictronParser
    {
        IList<IVictronMessage> Parse(byte[] bytes, VictronParserState state);
    }
}
