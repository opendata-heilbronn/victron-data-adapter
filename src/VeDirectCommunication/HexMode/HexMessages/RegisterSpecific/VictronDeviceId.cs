namespace VeDirectCommunication.HexMode.HexMessages.RegisterSpecific
{
    public enum VictronDeviceId
    {
        BlueSolarMPPT70_15 = 0x0300, // (*2 *3) 12..24V-15A 15A - 
        BlueSolarMPPT75_50 = 0xA040, // (*3) 12..24V-50A - pv only 
        BlueSolarMPPT150_35 = 0xA041, // (*3) 12..48V-35A - pv only 
        BlueSolarMPPT75_15 = 0xA042, // 12..24V-15A 15A - 
        BlueSolarMPPT100_15 = 0xA043, // 12..24V-15A 15A - 
        BlueSolarMPPT100_30 = 0xA044, // (*3) 12..24V-30A - pv only 
        BlueSolarMPPT100_50 = 0xA045, // (*3) 12..24V-50A - pv only 
        BlueSolarMPPT150_70 = 0xA046, //  12..48V-70A - - 
        BlueSolarMPPT150_100 = 0xA047, // 12..48V-100A - - 
        BlueSolarMPPT75_50_rev2 = 0xA048, //  (*3) 12..24V-50A - - 
        BlueSolarMPPT100_50_rev2 = 0xA049, // rev2 12..24V-50A - - 
        BlueSolarMPPT100_30_rev2 = 0xA04A, // rev2 12..24V-30A - - 
        BlueSolarMPPT150_35_rev2 = 0xA04B, // rev2 12..48V-35A - - 
        BlueSolarMPPT75_10 = 0xA04C, // 12..24V-10A 10A - 
        BlueSolarMPPT150_45 = 0xA04D, //  12..48V-45A - - 
        BlueSolarMPPT150_60 = 0xA04E, //  12..48V-60A - - 
        BlueSolarMPPT150_85 = 0xA04F, // 12..48V-85A - - 
        SmartSolarMPPT250_100 = 0xA050, // 12..48V-100A - ble 
        SmartSolarMPPT150_100 = 0xA051, // (*3 *4) 12..48V-100A - ble  
        SmartSolarMPPT150_85 = 0xA052, // (*3 *4) 12..48V-85A - ble 
        SmartSolarMPPT75_15 = 0xA053, // 12..24V-15A 15A ble net 
        SmartSolarMPPT75_10 = 0xA054, // 12..24V-10A 10A ble net 
        SmartSolarMPPT100_15 = 0xA055, // 12..24V-15A 15A ble net 
        SmartSolarMPPT100_30 = 0xA056, // 12..24V-30A - ble net 
        SmartSolarMPPT100_50 = 0xA057, // 12..24V-50A - ble net 
        SmartSolarMPPT150_35 = 0xA058, // 12..48V-35A - ble net 
        SmartSolarMPPT150_100_rev2 = 0xA059, // rev2 12..48V-100A - ble net 
        SmartSolarMPPT150_85_rev2 = 0xA05A, // rev2 12..48V-85A - ble net 
        SmartSolarMPPT250_70 = 0xA05B, // 12..48V-70A - ble net 
        SmartSolarMPPT250_85 = 0xA05C, // 12..48V-85A - ble 
        SmartSolarMPPT250_60 = 0xA05D, // 12..48V-60A - ble net 
        SmartSolarMPPT250_45 = 0xA05E, // 12..48V-45A - ble net 
        SmartSolarMPPT100_20 = 0xA05F, // 12..24V-20A 20A ble net 
        SmartSolarMPPT100_20_48V = 0xA060, // 48V 12..48V-20A 100mA ble net 
        SmartSolarMPPT150_45 = 0xA061, // 12..48V-45A - ble net 
        SmartSolarMPPT150_60 = 0xA062, // 12..48V-60A - ble net 
        SmartSolarMPPT150_70 = 0xA063, // 12..48V-70A - ble net 
        SmartSolarMPPT250_85_rev2 = 0xA064, // rev2 12..48V-85A - ble net 
        SmartSolarMPPT250_100_rev2 = 0xA065, // rev2 12..48V-100A - ble net 
    }
}
