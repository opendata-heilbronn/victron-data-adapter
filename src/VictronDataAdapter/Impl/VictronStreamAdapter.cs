using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Logging;
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
        private readonly IVictronParser messageParser;
        private readonly ILogger<VictronStreamAdapter> logger;
        private readonly VictronParserState parserState;

        public VictronStreamAdapter(IVictronParser messageParser, ILogger<VictronStreamAdapter> logger)
        {
            this.messageParser = messageParser;
            this.logger = logger;
            this.parserState = new VictronParserState();
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
                this.logger.LogInformation("No Text Message");
                return null;
            }

            if (!textMessage.ChecksumValid)
            {
                this.logger.LogWarning("Invalid Checksum!");
                return null;
            }

            this.logger.LogInformation("Got {MessageCount} Messages in Packet", textMessage.Messages.Count);
            foreach (var message in textMessage.Messages)
            {
                this.logger.LogDebug("Got Message with Key {MessageKey} Value {MessageValue}", message.Key, message.Value);

                MapMessage(message, dataPoint);
            }
            AppendData(dataPoint);

            return dataPoint;
        }

        private static void AppendData(Point dataPoint)
        {
            dataPoint.Fields["ChargeCurrent"] = (double.Parse((string)dataPoint.Fields["BatteryCurrent"], CultureInfo.InvariantCulture) + double.Parse((string)dataPoint.Fields["LoadCurrent"], CultureInfo.InvariantCulture)).ToString("0.00###", CultureInfo.InvariantCulture);
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
                    dataPoint.Fields["BatteryVoltage"] = FormatMilli(message.Value);
                    break;
                case "I":
                    dataPoint.Fields["BatteryCurrent"] = FormatMilli(message.Value);
                    break;
                case "VPV":
                    dataPoint.Fields["SolarVoltage"] = FormatMilli(message.Value);
                    break;
                case "PPV":
                    dataPoint.Fields["SolarPower"] = message.Value;
                    break;
                case "CS":
                    dataPoint.Fields["ChargeState"] = message.Value;
                    break;
                case "ERR":
                    dataPoint.Fields["ErrorCode"] = message.Value;
                    break;
                case "LOAD":
                    dataPoint.Fields["LoadOutputState"] = FormatOnOff(message.Value);
                    break;
                case "IL":
                    dataPoint.Fields["LoadCurrent"] = FormatMilli(message.Value);
                    break;
                case "MPPT":
                    dataPoint.Fields["MpptState"] = message.Value;
                    break;
                case "H19":
                case "H20":
                case "H21":
                case "H22":
                case "H23":
                case "HSDS":
                    break;
                default:
                    this.logger.LogWarning("Unsupported Message Key {MessageKey}", message.Key);
                    break;
            }
        }

        private string FormatOnOff(string value)
        {
            if (value == "ON")
                return "1";
            if (value == "OFF")
                return "0";
            this.logger.LogError("Invalid OnOff Value {Value}", value);
            return "-1";
        }

        private string FormatMilli(string value)
        {
            if (!int.TryParse(value, out var parsed))
            {
                this.logger.LogError("Invalid int value {Value}", parsed);
                return "0";
            }

            return (int.Parse(value) / 1000d).ToString("0.00###", CultureInfo.InvariantCulture);
        }

        private async Task<IList<IVictronMessage>> GetMessages(IDataReader reader)
        {
            while (true)
            {
                await reader.WaitForAvailable();
                var data = await reader.ReadAvailable();
                this.logger.LogInformation("Got {0} bytes", data.Length);

                var parsed = this.messageParser.Parse(data, this.parserState);
                if (parsed.Count > 0)
                    return parsed;
            }
        }
    }
}
