using System.IO;
using System.Threading.Tasks;

namespace VictronDataAdapter.Contracts
{
    public interface IVictronStreamAdapter
    {
        Task<VictronDataPoint> GetNextDataPoint(IDataReader reader);
    }
}