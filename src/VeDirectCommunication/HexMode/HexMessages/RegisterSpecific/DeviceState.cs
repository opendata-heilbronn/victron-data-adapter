namespace VeDirectCommunication.HexMode.HexMessages.RegisterSpecific
{
    public enum DeviceState
    {
        NotCharging = 0,
        Fault = 2,
        Bulk = 3,
        Absorption = 4,
        Float = 5,
        Equalize = 7,
        Ess = 252, // Remote voltage set-point
        Unavailable = 255
    }
}
