using System.Collections.Generic;

namespace VictronDataAdapter.Contracts
{
    public class VictronDataPoint
    {
        public IList<AdaptedMessage> Messages { get; set; }
    }
}
