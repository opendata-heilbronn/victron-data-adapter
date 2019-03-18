using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Logging;
using System;
using VeDirectCommunication.TextMode;

namespace VictronDataAdapter
{
    public class VictronStreamAdapter : IVictronStreamAdapter
    {
        private readonly ILogger<VictronStreamAdapter> _logger;

        public VictronStreamAdapter(ILogger<VictronStreamAdapter> logger)
        {
            _logger = logger;
        }

        public Point GetNextDataPoint(VictronTextBlock textBlock)
        {
            var dataPoint = new Point
            {
                Name = "solar"
            };
            
            dataPoint.Timestamp = DateTime.UtcNow;

            if (textBlock == null)
            {
                _logger.LogInformation("No Text Message");
                return null;
            }

            if (!textBlock.ChecksumValid)
            {
                _logger.LogWarning("Invalid Checksum!");
                return null;
            }
            foreach (var field in textBlock.Fields)
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
            dataPoint.Fields["ChargeCurrent"] = (double)dataPoint.Fields["BatteryCurrent"] + (double)dataPoint.Fields["LoadCurrent"];
            dataPoint.Fields["LoadPower"] = (double)dataPoint.Fields["BatteryVoltage"] * (double)dataPoint.Fields["LoadCurrent"];
        }

        private void MapMessage(VictronTextField field, Point dataPoint)
        {
            switch (field.Key)
            {
                case "PID":
                    dataPoint.Tags["ProductId"] = field.Value;
                    break;
                case "FW":
                    dataPoint.Fields["FirmwareVersion"] = field.Value;
                    break;
                case "SER#":
                    dataPoint.Tags["SerialNumber"] = field.Value;
                    break;
                case "V":
                    dataPoint.Fields["BatteryVoltage"] = ParseMilli(field.Value);
                    break;
                case "I":
                    dataPoint.Fields["BatteryCurrent"] = ParseMilli(field.Value);
                    break;
                case "VPV":
                    dataPoint.Fields["SolarVoltage"] = ParseMilli(field.Value);
                    break;
                case "PPV":
                    dataPoint.Fields["SolarPower"] = ParseInt(field.Value);
                    break;
                case "CS":
                    dataPoint.Fields["ChargeState"] = ParseInt(field.Value);
                    break;
                case "ERR":
                    dataPoint.Fields["ErrorCode"] = ParseInt(field.Value);
                    break;
                case "LOAD":
                    dataPoint.Fields["LoadOutputState"] = FormatOnOff(field.Value);
                    break;
                case "IL":
                    dataPoint.Fields["LoadCurrent"] = ParseMilli(field.Value);
                    break;
                case "MPPT":
                    dataPoint.Fields["MpptState"] = ParseInt(field.Value);
                    break;
                case "H19":
                    dataPoint.Fields["YieldTotal"] = ParseInt(field.Value) / 100.0;
                    break;
                case "H20":
                case "H21":
                case "H22":
                case "H23":
                case "HSDS":
                    break;
                default:
                    _logger.LogWarning("Unsupported Message Key {MessageKey}", field.Key);
                    break;
            }
        }

        private string FormatOnOff(string value)
        {
            switch (value)
            {
                case "ON":
                    return "1";
                case "OFF":
                    return "0";
                default:
                    _logger.LogError("Invalid OnOff Value {Value}", value);
                    return "-1";
            }
        }

        private double ParseMilli(string value)
        {
            if (!int.TryParse(value, out var parsed))
            {
                throw new FormatException($"Invalid int value {value}");
            }

            return parsed / 1000d;
        }

        private int ParseInt(string value)
        {
            if (!int.TryParse(value, out var parsed))
            {
                throw new FormatException($"Invalid int value {value}");
            }

            return parsed;
        }
    }
}
