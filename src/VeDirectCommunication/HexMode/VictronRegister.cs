namespace VeDirectCommunication.HexMode
{
    public enum VictronRegister : ushort
    {
        //Product information
        ProductId = 0x0100,
        GroupId = 0x0104,
        DeviceInstance = 0x0105,
        Capabilities = 0x0140,

        //Generic device control
        DeviceMode = 0x0200,
        DeviceState = 0x0201,
        RemoteControlUsed = 0x0202,
        DeviceOffReason = 0x0205,

        //Battery settings
        BatterysafeMode = 0xEDFF,
        AutomaticEqualizationMode = 0xEDFD,
        BatteryBulkTimeLimit = 0xEDFC,
        BatteryAbsorptionTimeLimit = 0xEDFB,
        BatteryAbsorptionVoltage = 0xEDF7,
        BatteryFloatVoltage = 0xEDF6,
        BatteryEqualizationVoltage = 0xEDF4,
        BatteryTempCompensation = 0xEDF2,
        BatteryType = 0xEDF1,
        BatteryMaxCurrent = 0xEDF0,
        BatteryLowTemperature = 0xEDE0,
        BatteryVoltage = 0xEDEF,
        BatteryTemperature = 0xEDEC,
        BatteryVoltageSetting = 0xEDEA,
        BmsPresent = 0xEDE8,
        LowTemperatureChargeCurrent = 0xEDE6,

        //Charger data
        ChargerMaximumCurrent = 0xEDDF,
        SystemYield = 0xEDDD,
        UserYield = 0xEDDC,
        ChargerTemperature = 0xEDDB,
        ChargerErrorCode = 0xEDDA,
        ChargerCurrent = 0xEDD7,
        ChargerVoltage = 0xEDD5,
        AdditionalChargerState = 0xEDD4,
        YieldToday = 0xEDD3,
        MaximumPowerToday = 0xEDD2,
        YieldYesterday = 0xEDD1,
        MaximumPowerYesterday = 0xEDD0,
        VoltageSettingsRange = 0xEDCE,
        HistoryVersion = 0xEDCD,
        StreetlightVersion = 0xEDCC,
        AdjustableVoltageMinimum = 0x2211,
        AdjustableVoltageMaximum = 0x2212,

        //Solar panel data
        PanelPower = 0xEDBC,
        PanelVoltage = 0xEDBB,
        PanelCurrent = 0xEDBD,
        PanelMaximumVoltage = 0xEDB8,

        //load output data/settings
        LoadCurrent = 0xEDAD,
        LoadOffsetVoltage = 0xEDAC,
        LoadOutputControl = 0xEDAB,
        LoadOutputVoltage = 0xEDA9,
        LoadOutputState = 0xEDA8,
        LoadSwitchHighLevel = 0xED9D,
        LoadSwitchLowLevel = 0xED9C,
        LoadOutputOffReason = 0xED91,

        //Relay settings
        //[...]

        //Lighting controller timer
        //[...]

        //Ve.Direct port functions
        TxPortOperationMode = 0xED9E,
        RxPortOperationMode = 0xED98,

        //Restore factory defaults
        RestoreFactoryDefaults = 0x0004,

        //History data
        ClearHistory = 0x1030,
        TotalHistory = 0x104F,

        DailyHistoryToday = 0x1050,
        DailyHistoryYesterday = 0x1051,
        //... until 0x106E

        //Pluggable display settings
        //[...]

        //Remote control
        //[...]
    }
}
