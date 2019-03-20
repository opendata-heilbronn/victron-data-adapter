using System;
using System.Collections.Generic;
using VeDirectCommunication.HexMode.Registers;

namespace VeDirectCommunication.HexMode
{
    public static class SupportedAsyncRegisters
    {
        public static IEnumerable<VictronRegister> Get(VictronVersion version)
        {
            if (version.VersionNumber >= new Version(1, 16))
            {
                yield return VictronRegister.TotalHistory;
                yield return VictronRegister.DailyHistoryToday;
            }
            if (version.VersionNumber >= new Version(1, 17))
            {
                yield return VictronRegister.DeviceMode;
                yield return VictronRegister.DeviceState;
                yield return VictronRegister.RemoteControlUsed;
                yield return VictronRegister.DeviceOffReason;
                yield return VictronRegister.ChargerCurrent;
                yield return VictronRegister.AdditionalChargerState;
                yield return VictronRegister.PanelVoltage;
                yield return VictronRegister.LoadCurrent;
                yield return VictronRegister.LoadOutputVoltage;
                yield return VictronRegister.LoadOutputState;
                yield return VictronRegister.LoadOutputOffReason;
                yield return VictronRegister.SwitchBankStatus;
                yield return VictronRegister.AbsorptionTimeLeft;
            }
            if (version.VersionNumber >= new Version(1, 26))
            {
                yield return VictronRegister.ChargerVoltage;
                yield return VictronRegister.SolarActivity;
                yield return VictronRegister.BatteryVoltageSense;
                yield return VictronRegister.BatteryTemperatureSense;
            }
        }
    }
}
