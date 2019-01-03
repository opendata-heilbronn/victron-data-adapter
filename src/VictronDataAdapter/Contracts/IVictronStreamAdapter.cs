using InfluxData.Net.InfluxDb.Models;
using System.Threading.Tasks;

namespace VictronDataAdapter.Contracts
{
    public interface IVictronStreamAdapter
    {
        Task<Point> GetNextDataPoint(IDataReader reader);
    }
}