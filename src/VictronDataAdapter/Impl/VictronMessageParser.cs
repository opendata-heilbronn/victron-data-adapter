using Microsoft.Extensions.Logging;
using System;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter.Impl
{
    public class VictronMessageParser : IVictronMessageParser
    {
        private readonly ILogger<VictronMessageParser> logger;

        public VictronMessageParser(ILogger<VictronMessageParser> logger)
        {
            this.logger = logger;
        }

        public VictronMessage ParseLine(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            var splitLine = line.Split('\t');
            if (splitLine.Length != 2)
            {
                this.logger.LogError("Invalid Line {line}", line);
                return null;
            }

            return new VictronMessage
            {
                Key = splitLine[0].Trim(),
                Value = splitLine[1].Trim()
            };
        }
    }
}
