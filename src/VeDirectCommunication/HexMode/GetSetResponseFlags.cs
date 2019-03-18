using System;

namespace VeDirectCommunication.HexMode
{
    [Flags]
    public enum GetSetResponseFlags
    {
        None = 0x00,
        UnknownId = 0x01,
        NotSupported = 0x02,
        ParameterError = 0x04
    }
}
