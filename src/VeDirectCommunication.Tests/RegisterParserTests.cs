using System;
using VeDirectCommunication.HexMode.HexMessages.RegisterSpecific;
using Xunit;

namespace VeDirectCommunication.Tests
{
    public class RegisterParserTests
    {
        [Fact]
        public void Parse_PingResponse()
        {
            var sut = new RegisterParser();

            var result = sut.ParsePingResponse(new byte[] { 0x16, 0x41 });

            Assert.Equal(FirmwareType.Application, result.FirmwareType);
            Assert.Equal(Version.Parse("1.16"), result.VersionNumber);
        }

        [Fact]
        public void Parse_Unsigned16()
        {
            var sut = new RegisterParser();

            var result = sut.ParseUInt16(new byte[] { 0x96, 0x00 });

            Assert.Equal(150, result);
        }

        [Fact]
        public void Parse_Signed16_Positive()
        {
            var sut = new RegisterParser();

            var result = sut.ParseSInt16(new byte[] { 0xA0, 0x0F });

            Assert.Equal(4000, result);
        }

        [Fact]
        public void Parse_Signed16_Negative()
        {
            var sut = new RegisterParser();

            var result = sut.ParseSInt16(new byte[] { 0x48, 0xF4 });

            Assert.Equal(-3000, result);
        }

        [Fact]
        public void Parse_Unsigned32_One()
        {
            var sut = new RegisterParser();

            var result = sut.ParseUInt32(new byte[] { 0x01, 0x00, 0x00, 0x00 });

            Assert.Equal(1U, result);
        }

        [Fact]
        public void Parse_Unsigned32_Max()
        {
            var sut = new RegisterParser();

            var result = sut.ParseUInt32(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            Assert.Equal(uint.MaxValue, result);
        }

        [Fact]
        public void Parse_String()
        {
            var sut = new RegisterParser();

            var result = sut.ParseString(new byte[] { 0x53, 0x6D, 0x61, 0x72, 0x74, 0x53, 0x6F, 0x6C, 0x61, 0x72, 0x20, 0x43, 0x68, 0x61, 0x72, 0x67, 0x65, 0x72, 0x20, 0x4D, 0x50, 0x50, 0x54, 0x20, 0x31, 0x30, 0x30, 0x2F, 0x31, 0x35, 0x00 });

            Assert.Equal("SmartSolar Charger MPPT 100/15", result);
        }
    }
}
