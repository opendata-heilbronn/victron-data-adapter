﻿using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VeDirectCommunication.HexMode;
using VeDirectCommunication.HexMode.Registers;

namespace VictronDataAdapter
{
    public class VictronStreamAdapter : IVictronStreamAdapter
    {
        private readonly ILogger<VictronStreamAdapter> _logger;
        private readonly RegisterParser _registerParser;

        public VictronStreamAdapter(ILogger<VictronStreamAdapter> logger, RegisterParser registerParser)
        {
            _logger = logger;
            _registerParser = registerParser;
        }

        public Point GetNextDataPoint(IDictionary<VictronRegister, byte[]> registers)
        {
            var dataPoint = new Point
            {
                Name = "solar"
            };
            
            dataPoint.Timestamp = DateTime.UtcNow;
            
            foreach (var field in registers)
            {
                _logger.LogDebug("Got Message with Key {MessageKey} Value {MessageValue}", field.Key, field.Value);

                try
                {
                    MapMessage(field, dataPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to map message with key {MessageKey}", field.Key);
                }
            }
            AppendAdditionalData(dataPoint);

            return dataPoint;
        }

        private static void AppendAdditionalData(Point dataPoint)
        {
            dataPoint.Fields["LoadPower"] = (double)dataPoint.Fields["LoadOutputVoltage"] * (double)dataPoint.Fields["LoadCurrent"];
            dataPoint.Fields["BatteryCurrent"] = (double)dataPoint.Fields["ChargeCurrent"] - (double)dataPoint.Fields["LoadCurrent"];
        }

        private void MapMessage(KeyValuePair<VictronRegister, byte[]> field, Point dataPoint)
        {
            switch (field.Key)
            {
                //case "PID":
                //    dataPoint.Tags["ProductId"] = field.Value;
                //    break;
                //case "FW":
                //    dataPoint.Fields["FirmwareVersion"] = field.Value;
                //    break;
                case VictronRegister.SerialNumber:
                    dataPoint.Tags["SerialNumber"] = _registerParser.ParseString(field.Value);
                    break;
                case VictronRegister.ChargerVoltage:
                    dataPoint.Fields["BatteryVoltage"] = _registerParser.ParseUInt16(field.Value) * 0.01;
                    break;
                case VictronRegister.ChargerCurrent:
                    dataPoint.Fields["ChargeCurrent"] = _registerParser.ParseUInt16(field.Value) * 0.1;
                    break;
                case VictronRegister.PanelVoltage:
                    dataPoint.Fields["SolarVoltage"] = _registerParser.ParseUInt16(field.Value) * 0.01;
                    break;
                case VictronRegister.PanelPower:
                    dataPoint.Fields["SolarPower"] = (int)(_registerParser.ParseUInt32(field.Value) * 0.01);
                    dataPoint.Fields["SolarPowerNew"] = _registerParser.ParseUInt32(field.Value) * 0.01;
                    break;
                case VictronRegister.DeviceState:
                    dataPoint.Fields["ChargeState"] = (int)field.Value[0];
                    break;
                case VictronRegister.ChargerErrorCode:
                    dataPoint.Fields["ErrorCode"] = (int)field.Value[0];
                    break;
                case VictronRegister.LoadOutputState:
                    dataPoint.Fields["LoadOutputState"] = field.Value[0] != 0 ? "1" : "0";
                    break;
                case VictronRegister.LoadOutputVoltage:
                    dataPoint.Fields["LoadOutputVoltage"] = _registerParser.ParseUInt16(field.Value) * 0.01;
                    break;
                case VictronRegister.LoadCurrent:
                    dataPoint.Fields["LoadCurrent"] = _registerParser.ParseUInt16(field.Value) * 0.1;
                    break;
                //case "MPPT":
                //    dataPoint.Fields["MpptState"] = ParseInt(field.Value);
                //    break;
                case VictronRegister.SystemYield:
                    dataPoint.Fields["YieldTotal"] = _registerParser.ParseUInt32(field.Value) * 0.01;
                    break;
                case VictronRegister.ChargerTemperature:
                    dataPoint.Fields["ChargerTemperature"] = _registerParser.ParseSInt16(field.Value) * 0.01;
                    break;
                case VictronRegister.DeviceOffReason:
                    dataPoint.Fields["DeviceOffReason"] = field.Value[0];
                    break;
                default:
                    _logger.LogWarning("Unsupported Message Key {MessageKey}", field.Key);
                    break;
            }
        }
    }
}
