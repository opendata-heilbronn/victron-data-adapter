using System;

namespace VictronDataAdapter
{
    internal class VictronMessageParser
    {
        public VictronMessage ParseLine(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));
            var splitLine = line.Split('\t');
            if (splitLine.Length != 2)
                throw new ArgumentException($"Invalid Line {line}", nameof(line));

            return new VictronMessage
            {
                Key = splitLine[0].Trim(),
                Value = splitLine[1].Trim()
            };
        }
    }

    public class VictronMessage
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
