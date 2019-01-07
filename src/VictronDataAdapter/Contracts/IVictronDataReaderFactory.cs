using System;

namespace VictronDataAdapter.Contracts
{
    interface IVictronDataReaderFactory : IDisposable
    {
        IDataReader GetDataReader();
    }
}
