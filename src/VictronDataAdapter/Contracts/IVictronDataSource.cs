using System;

namespace VictronDataAdapter.Contracts
{
    interface IVictronDataSource : IDisposable
    {
        IDataReader GetDataReader();
    }
}
