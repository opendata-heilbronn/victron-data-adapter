using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using VictronDataAdapter.Contracts.VictronParser;

namespace VictronDataAdapter.Impl
{
    public class VictronParser : IVictronParser
    {
        private readonly ILogger<VictronParser> _logger;
        private const string ChecksumTagName = "Checksum";

        public VictronParser(ILogger<VictronParser> logger)
        {
            _logger = logger;
        }

        public IList<IVictronMessage> Parse(byte[] bytes, VictronParserState state)
        {
            var toReturn = new List<IVictronMessage>();
            foreach (var inbyte in bytes)
            {
                var message = ParseByte(inbyte, state);
                if (message != null)
                    toReturn.Add(message);
            }
            return toReturn;
        }

        // Heavily inspired by
        // https://www.victronenergy.com/live/vedirect_protocol:faq#framehandler_reference_implementation
        public IVictronMessage ParseByte(byte inbyte, VictronParserState state)
        {
            if ((inbyte == ':') && (state.ParseState != ParseState.Checksum))
            {
                state.ParseState = ParseState.HexRecord;
            }
            if (state.ParseState != ParseState.HexRecord)
            {
                state.Checksum += inbyte;
            }

            switch (state.ParseState)
            {
                case ParseState.Idle:
                    /* wait for \n of the start of an record */
                    switch (inbyte)
                    {
                        case (byte)'\n':
                            state.ParseState = ParseState.RecordBegin;
                            break;
                        case (byte)'\r': /* Skip */
                        default:
                            break;
                    }
                    break;
                case ParseState.RecordBegin:
                    state.RecordName = "";
                    state.RecordName += (char)inbyte;
                    state.ParseState = ParseState.RecordName;
                    break;
                case ParseState.RecordName:
                    // The record name is being received, terminated by a \t
                    switch (inbyte)
                    {
                        case (byte)'\t':
                            // the Checksum record indicates a EOR
                            if (state.RecordName == ChecksumTagName)
                            {
                                state.ParseState = ParseState.Checksum;
                                break;
                            }
                            state.RecordValue = "";
                            state.ParseState = ParseState.RecordValue;
                            break;
                        default:
                            // add byte to name
                            state.RecordName += (char)inbyte;
                            break;
                    }
                    break;
                case ParseState.RecordValue:
                    // The record value is being received.  The \r indicates a new record.
                    switch (inbyte)
                    {
                        case (byte)'\n':
                            // forward record
                            state.Records.Add(new VictronTextMessage { Key = state.RecordName, Value = state.RecordValue });
                            state.ParseState = ParseState.RecordBegin;
                            break;
                        case (byte)'\r': /* Skip */
                            break;
                        default:
                            // add byte to value
                            state.RecordValue += (char)inbyte;
                            break;
                    }
                    break;
                case ParseState.Checksum:
                    {
                        bool valid = state.Checksum == 0;
                        if (!valid)
                            this._logger.LogError($"Invalid frame checksum 0x{state.Checksum:X2}");
                        state.Checksum = 0;
                        state.ParseState = ParseState.Idle;
                        var records = state.Records;
                        state.Records = new List<VictronTextMessage>();
                        return new VictronTextBlock
                        {
                            ChecksumValid = valid,
                            Messages = records
                        };
                    }
                case ParseState.HexRecord:
                    switch (inbyte)
                    {
                        case (byte)'\n':
                            state.Checksum = 0;
                            state.ParseState = ParseState.Idle;
                            return new VictronHexMessage
                            {
                                Bytes = state.HexRecordBytes
                            };
                        case (byte)':':
                        case (byte)'\r': /* Skip */
                            break;
                        default:
                            // add byte to value
                            var nibble = Convert.ToByte(((char)inbyte).ToString(), 16);
                            if (state.LowNibbleSet)
                            {
                                state.LowNibbleSet = false;
                                var hexByte = (byte)(nibble | (state.LowNibble << 4));
                                state.HexRecordBytes.Add(hexByte);
                            }
                            else
                            {
                                state.LowNibble = nibble;
                                state.LowNibbleSet = true;
                            }
                            break;
                    }
                    break;
            }
            return null;
        }
    }
}
