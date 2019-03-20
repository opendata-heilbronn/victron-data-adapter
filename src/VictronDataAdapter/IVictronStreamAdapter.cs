using InfluxData.Net.InfluxDb.Models;
using System.Collections.Generic;
using VeDirectCommunication.HexMode;

namespace VictronDataAdapter
{
    public interface IVictronStreamAdapter
    {
        Point GetNextDataPoint(IDictionary<VictronRegister, byte[]> registers);
    }
}