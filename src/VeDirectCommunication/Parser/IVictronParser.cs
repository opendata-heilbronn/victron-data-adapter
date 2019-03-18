using System.Collections.Generic;

namespace VeDirectCommunication.Parser
{
    internal interface IVictronParser
    {
        IList<IVictronMessage> Parse(byte[] bytes, VictronParserState state);
    }
}
