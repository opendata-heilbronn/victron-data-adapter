using Microsoft.Extensions.Logging.Abstractions;
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
            var input = @"PID	0xA043
FW	119
SER#	HQXXXXXXXXX
V	13791
I	-10
VPV	15900
PPV	0
CS	5
ERR	0
LOAD	ON
IL	0
H19	0
H20	0
H21	397
H22	0
H23	0
HSDS	0
Checksum	l:A0002000148";
            var reader = new MockDataReader(input);
            var adapter = new VictronStreamAdapter(new VictronMessageParser(NullLogger<VictronMessageParser>.Instance), NullLogger<VictronStreamAdapter>.Instance);

            var point = await adapter.GetNextDataPoint(reader);

            Assert.Equal("13.791", point.Messages.Single(x => x.Key == "BatteryVoltage").Value);
            Assert.Equal("-0.01", point.Messages.Single(x => x.Key == "BatteryCurrent").Value);
            Assert.Equal("15.90", point.Messages.Single(x => x.Key == "SolarVoltage").Value);
            Assert.Equal("1", point.Messages.Single(x => x.Key == "LoadOutputState").Value);
            Assert.Equal("0.00", point.Messages.Single(x => x.Key == "LoadCurrent").Value);
        }
    }
}
