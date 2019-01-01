using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VictronDataAdapter
{
    internal class VictronStreamAdapter
    {
        private readonly TextReader reader;
        private readonly VictronMessageParser messageParser;
        private readonly ILogger<VictronStreamAdapter> logger;

        public VictronStreamAdapter(VictronMessageParser messageParser, ILogger<VictronStreamAdapter> logger)
        {
            this.messageParser = messageParser;
            this.logger = logger;
        }

        public async Task<VictronDataPoint> GetNextDataPoint(TextReader reader)
        {
            var aggregatedMessages = await GetAggregatedMessages(reader);
            this.logger.LogInformation("Got {MessageCount} Messages in Packet", aggregatedMessages.Count);

            var dataPoint = new VictronDataPoint
            {
                Messages = new List<AdaptedMessage>()
            };
            foreach (var message in aggregatedMessages)
            {
                this.logger.LogDebug("Got Message with Key {MessageKey} Value {MessageValue}", message.Key, message.Value);

                var mappedMessage = MapMessage(message);
                if (mappedMessage == null)
                    continue;
                dataPoint.Messages.Add(mappedMessage);
            }
            return dataPoint;
        }

        private AdaptedMessage MapMessage(VictronMessage message)
        {
            switch (message.Key)
            {
                case "PID":
                    return new AdaptedMessage { Key = "ProductId", Value = message.Value };
                case "FW":
                    return new AdaptedMessage { Key = "FirmwareVersion", Value = message.Value };
                case "SER#":
                    return new AdaptedMessage { Key = "SerialNumber", Value = message.Value };
                case "V":
                    return new AdaptedMessage { Key = "BatteryVoltage", Value = FormatMilli(message.Value) };
                case "I":
                    return new AdaptedMessage { Key = "BatteryCurrent", Value = FormatMilli(message.Value) };
                case "VPV":
                    return new AdaptedMessage { Key = "SolarVoltage", Value = FormatMilli(message.Value) };
                case "PPV":
                    return new AdaptedMessage { Key = "SolarPower", Value = message.Value };
                case "CS":
                    return new AdaptedMessage { Key = "ChargeState", Value = message.Value };
                case "ERR":
                    return new AdaptedMessage { Key = "ErrorCode", Value = message.Value };
                case "LOAD":
                    return new AdaptedMessage { Key = "LoadOutputState", Value = FormatOnOff(message.Value) };
                case "IL":
                    return new AdaptedMessage { Key = "LoadCurrent", Value = FormatMilli(message.Value) };
                default:
                    this.logger.LogDebug("Unsupported Message Key {MessageKey}", message.Key);
                    return null;
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
            if(!int.TryParse(value, out var parsed))
            {
                this.logger.LogError("Invalid int value {Value}", parsed);
                return "0";
            }

            return (int.Parse(value) / 1000d).ToString("0.0", CultureInfo.InvariantCulture);
        }

        private async Task<IList<VictronMessage>> GetAggregatedMessages(TextReader reader)
        {
            var lines = new List<string>();

            var line = await reader.ReadLineAsync();

            while (line != null)
            {
                lines.Add(line);
                line = await ReadLineWithTimeout(reader, 50);
            }

            return lines.Select(x => this.messageParser.ParseLine(x)).ToList();
        }

        private async Task<string> ReadLineWithTimeout(TextReader reader, int timeout)
        {
            var startTime = DateTime.UtcNow;
            do
            {
                var line = await reader.ReadLineAsync();
                if (line != null)
                    return line;

                await Task.Delay(10);
            } while ((DateTime.UtcNow - startTime).TotalMilliseconds > timeout);

            return null;
        }
    }

    public class VictronDataPoint
    {
        public IList<AdaptedMessage> Messages { get; set; }
    }

    public class AdaptedMessage
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
