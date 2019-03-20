using System;
using System.Collections.Generic;
using System.Linq;
using VeDirectCommunication.HexMode.HexMessages;

namespace VeDirectCommunication.HexMode
{
    internal class VictronHexMessageSerializer : IVictronHexMessageSerializer
    {
        public string SerializeGetRegister(VictronRegister register)
        {
            var bytes = new byte[3];
            Array.Copy(GetUInt16((ushort)register), bytes, 2);

            return Serialize(HexCommand.Get, bytes);
        }

        public string Serialize(HexCommand command, byte[] data)
        {
            var checksum = GetChecksum((byte)command, data);
            var commandString = $":{((int)command):X1}{string.Join("", data.Select(x => x.ToString("X2")))}{checksum:X2}\n";
            return commandString;
        }

        public IHexResponseMessage ParseHexMessage(byte[] messageNibbles)
        {
            if (messageNibbles.Length < 2)
                throw new ArgumentException("Invalid Hex Messages");
            var responseType = (HexResponse)messageNibbles[0];
            var bytes = GetBytesFromNibbles(messageNibbles.Skip(1).ToList());

            var dataBytes = new byte[bytes.Length - 1];
            Array.Copy(bytes, dataBytes, bytes.Length - 1);

            var expectedChecksum = GetChecksum((byte)responseType, dataBytes);
            var actualChecksum = bytes.Last();

            if (expectedChecksum != actualChecksum)
                throw new ArgumentException($"Invalid Checksum! Expected {expectedChecksum:X2} Got {actualChecksum:X2}");

            switch (responseType)
            {
                case HexResponse.Get:
                    return HandleRegisterValueResponse<GetRegisterResponseMessage>(dataBytes);
                case HexResponse.Async:
                    return HandleRegisterValueResponse<AsyncRegisterResponseMessage>(dataBytes);
                case HexResponse.Ping:
                    return HandlePingResponse(dataBytes);
                case HexResponse.Done:
                case HexResponse.Unknown:
                case HexResponse.Error:
                case HexResponse.Set:
                default:
                    throw new ArgumentException($"Unhandled Response Type {responseType}");
            }
        }

        private byte[] GetBytesFromNibbles(IList<byte> nibbles)
        {
            if (nibbles.Count % 2 != 0)
                throw new ArgumentException("Number of Nibbles must be even to make bytes");

            var toReturn = new List<byte>();
            for(int i = 0; i < nibbles.Count; i += 2)
            {
                toReturn.Add((byte)((nibbles[i] << 4) | nibbles[i+1]));
            }

            return toReturn.ToArray();
        }

        private T HandleRegisterValueResponse<T>(byte[] dataBytes)
            where T : RegisterValueResponseMessage, new()
        {
            if (dataBytes.Length < 3)
                throw new ArgumentException("Get Response must have at least id and flags");
            var id = (VictronRegister)(dataBytes[0] | (dataBytes[1] << 8));
            var flags = (GetSetResponseFlags)dataBytes[2];
            var data = new List<byte>();

            for (int i = 3; i < dataBytes.Length; i++)
            {
                data.Add(dataBytes[i]);
            }

            return new T
            {
                Register = id,
                Flags = flags,
                RegisterValue = data.ToArray()
            };
        }

        private IHexResponseMessage HandlePingResponse(byte[] dataBytes)
        {
            return new PingResponseMessage
            {
                Version = dataBytes
            };
        }

        private byte GetChecksum(byte command, byte[] data)
        {
            byte checksum = 0x55;

            //wtf, original documentation says only data bytes go into checksum, but that is wrong
            checksum -= (byte)command;

            foreach (var dataByte in data)
            {
                checksum -= dataByte;
            }

            return checksum;
        }

        private byte[] GetUInt16(ushort n)
        {
            //wtf, documentation says bytes are little endian, but that is also wrong...
            return new byte[] { (byte)(n & 0xFF), (byte)(n >> 8 & 0xFF) };
        }
    }
}
