using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VictronDataAdapter.Impl;
using Xunit;

namespace VictronDataAdapter.Tests
{
    public class VictronStreamAdapterTests
    {
        [Fact]
        public async Task TestPacket_Parsed_FieldValuesCorrect()
        {
            var input = File.ReadAllBytes("SerialExample.txt");
            var reader = new MockDataReader(input);
            var adapter = new VictronStreamAdapter(new VictronParser(NullLogger<VictronParser>.Instance), NullLogger<VictronStreamAdapter>.Instance);

            var point = await adapter.GetNextDataPoint(reader);

            Assert.Equal("13.791", point.Fields.Single(x => x.Key == "BatteryVoltage").Value);
            Assert.Equal("-0.01", point.Fields.Single(x => x.Key == "BatteryCurrent").Value);
            Assert.Equal("15.90", point.Fields.Single(x => x.Key == "SolarVoltage").Value);
            Assert.Equal("1", point.Fields.Single(x => x.Key == "LoadOutputState").Value);
            Assert.Equal("0.00", point.Fields.Single(x => x.Key == "LoadCurrent").Value);
        }
    }
}
