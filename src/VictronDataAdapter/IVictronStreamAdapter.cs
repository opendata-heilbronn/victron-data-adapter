using InfluxData.Net.InfluxDb.Models;
using VeDirectCommunication.TextMode;

namespace VictronDataAdapter
{
    public interface IVictronStreamAdapter
    {
        Point GetNextDataPoint(VictronTextBlock textBlock);
    }
}