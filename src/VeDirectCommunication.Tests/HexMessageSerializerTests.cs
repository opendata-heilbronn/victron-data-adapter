using VeDirectCommunication.HexMode;
using VeDirectCommunication.HexMode.HexMessages;
using Xunit;

namespace VictronDataAdapter.Tests
{
    public class HexMessageSerializerTests
    {
        [Fact]
        public void Serialize_HexCommand()
        {
            var sut = new VictronHexMessageSerializer();

            var serialized = sut.Serialize(HexCommand.Get, new byte[] { 0xF0, 0xED, 0x00});

            Assert.Equal(":7F0ED0071", serialized);
        }

        [Fact]
        public void Serialize_GetCommand()
        {
            var sut = new VictronHexMessageSerializer();
            
            var serialized = sut.SerializeGetRegister(VictronRegister.BatteryMaxCurrent);

            Assert.Equal(":7F0ED0071", serialized);
        }

        [Fact]
        public void Deserialize_GetResponse()
        {
            var sut = new VictronHexMessageSerializer();
            var parsedMessage = sut.ParseHexMessage(new byte[] { 0x7, 0xF, 0x0, 0xE, 0xD, 0x0, 0x0, 0x9, 0x6, 0x0, 0x0, 0xD, 0xB, });

            Assert.True(parsedMessage is GetRegisterResponseMessage);

            var getMessage = (GetRegisterResponseMessage)parsedMessage;

            Assert.Equal(GetSetResponseFlags.None, getMessage.Flags);
            Assert.Equal(new byte[] { 0x96, 0x00 }, getMessage.RegisterValue);
        }


        //Get Model Name = :70B010071
        //Response = :70B0100536D617274536F6C61722043686172676572204D505054203130302F313500B7
    }
}
