using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Contracts.VictronParser;

namespace VictronDataAdapter.Impl
{
    public class VictronStreamAdapter : IVictronStreamAdapter
    {
        private readonly IVictronParser _messageParser;
        private readonly ILogger<VictronStreamAdapter> _logger;
        private readonly VictronParserState _parserState;

        public VictronStreamAdapter(IVictronParser messageParser, ILogger<VictronStreamAdapter> logger)
        {
            _messageParser = messageParser;
            _logger = logger;
            _parserState = new VictronParserState();
        }

        public async Task<Point> GetNextDataPoint(IDataReader reader)
        {
            var dataPoint = new Point
            {
                Name = "solar"
            };

            var messages = await GetMessages(reader);

            var textMessage = (VictronTextBlock)messages.LastOrDefault(x => x.MessageType == VictronMessageType.Text);
            if (textMessage == null)
            {
                _logger.LogInformation("No Text Message");
                return null;
            }

            if (!textMessage.ChecksumValid)
            {
                _logger.LogWarning("Invalid Checksum!");
                return null;
            }

            _logger.LogInformation("Got {MessageCount} Messages in Packet", textMessage.Messages.Count);
            foreach (var message in textMessage.Messages)
            {
                _logger.LogDebug("Got Message with Key {MessageKey} Value {MessageValue}", message.Key, message.Value);

                try
                {
                    MapMessage(message, dataPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to map message with key {MessageKey}", message.Key);
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

        private void MapMessage(VictronTextMessage message, Point dataPoint)
        {
            switch (message.Key)
            {
                case "PID":
                    dataPoint.Tags["ProductId"] = message.Value;
                    break;
                case "FW":
                    dataPoint.Fields["FirmwareVersion"] = message.Value;
                    break;
                case "SER#":
                    dataPoint.Tags["SerialNumber"] = message.Value;
                    break;
                case "V":
                    dataPoint.Fields["BatteryVoltage"] = ParseMilli(message.Value);
                    break;
                case "I":
                    dataPoint.Fields["BatteryCurrent"] = ParseMilli(message.Value);
                    break;
                case "VPV":
                    dataPoint.Fields["SolarVoltage"] = ParseMilli(message.Value);
                    break;
                case "PPV":
                    dataPoint.Fields["SolarPower"] = ParseInt(message.Value);
                    break;
                case "CS":
                    dataPoint.Fields["ChargeState"] = ParseInt(message.Value);
                    break;
                case "ERR":
                    dataPoint.Fields["ErrorCode"] = ParseInt(message.Value);
                    break;
                case "LOAD":
                    dataPoint.Fields["LoadOutputState"] = FormatOnOff(message.Value);
                    break;
                case "IL":
                    dataPoint.Fields["LoadCurrent"] = ParseMilli(message.Value);
                    break;
                case "MPPT":
                    dataPoint.Fields["MpptState"] = ParseInt(message.Value);
                    break;
                case "H19":
                    dataPoint.Fields["YieldTotal"] = ParseInt(message.Value) / 100.0;
                    break;
                case "H20":
                case "H21":
                case "H22":
                case "H23":
                case "HSDS":
                    break;
                default:
                    _logger.LogWarning("Unsupported Message Key {MessageKey}", message.Key);
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

        private async Task<IList<IVictronMessage>> GetMessages(IDataReader reader)
        {
            while (true)
            {
                await reader.WaitForAvailable();
                var data = await reader.ReadAvailable();
                _logger.LogInformation("Got {0} bytes", data.Length);

                var parsed = _messageParser.Parse(data, _parserState);
                if (parsed.Count > 0)
                    return parsed;
            }
        }
    }
}
