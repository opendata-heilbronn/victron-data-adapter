using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VeDirectCommunication.Parser;
using VeDirectCommunication.TextMode;
using Xunit;

namespace VictronDataAdapter.Tests
{
    public class VictronStreamAdapterTests
    {
        [Fact]
        public void TestPacket_Parsed_FieldValuesCorrect()
        {
            var input = File.ReadAllBytes("SerialExample.txt");
            var adapter = new VictronStreamAdapter(NullLogger<VictronStreamAdapter>.Instance);

            var parser = new VictronParser(new NullLogger<VictronParser>());
            var messages = parser.Parse(input, new VictronParserState());

            Assert.Equal(2, messages.Count);
            
            var point = adapter.GetNextDataPoint((VictronTextBlock)messages.Single(x => x.MessageType == VictronMessageType.Text));

            Assert.Equal(13.791, point.Fields.Single(x => x.Key == "BatteryVoltage").Value);
            Assert.Equal(-0.01, point.Fields.Single(x => x.Key == "BatteryCurrent").Value);
            Assert.Equal(15.90, point.Fields.Single(x => x.Key == "SolarVoltage").Value);
            Assert.Equal("1", point.Fields.Single(x => x.Key == "LoadOutputState").Value);
            Assert.Equal(0.00, point.Fields.Single(x => x.Key == "LoadCurrent").Value);
        }
    }
}
