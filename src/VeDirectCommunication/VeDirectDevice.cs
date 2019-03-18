using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VeDirectCommunication.Exceptions;
using VeDirectCommunication.HexMode;
using VeDirectCommunication.HexMode.HexMessages;
using VeDirectCommunication.Parser;
using VeDirectCommunication.TextMode;

namespace VeDirectCommunication
{
    internal class VeDirectDevice : IVeDirectDevice
    {
        public event TextMessageReceivedHandler TextMessageReceived;
        public event AsyncMessageReceivedHandler AsyncMessageReceived;

        private const int _deviceResponseTimeout = 10000;

        private readonly IVictronStream _victronStream;
        private readonly ILogger<VeDirectDevice> _logger;
        private readonly IVictronParser _parser;
        private readonly IVictronHexMessageSerializer _hexSerializer;

        private readonly IList<PendingGetResponse> _pendingGetResponses = new List<PendingGetResponse>();
        private PendingPingResponse _pendingPingResponse = null;

        private bool _running;
        private VictronParserState _parserState;
        private Thread _readThread;
        private SemaphoreSlim _startStopLock = new SemaphoreSlim(1);
        private SemaphoreSlim _writeLock = new SemaphoreSlim(1);
        
        public VeDirectDevice(IVictronStream victronStream, ILogger<VeDirectDevice> logger, IVictronParser parser, IVictronHexMessageSerializer hexMessageSerializer)
        {
            _victronStream = victronStream;
            _logger = logger;
            _parser = parser;
            _hexSerializer = hexMessageSerializer;
        }

        public async Task Start()
        {
            await _startStopLock.WaitAsync();

            try
            {
                if (_running)
                    return;
                _parserState = new VictronParserState();
                _readThread = new Thread(ReadThreadWorker);
                _readThread.Start();
                _running = true;
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        private async void ReadThreadWorker()
        {
            while (_running)
            {
                try
                {
                    if (!await _victronStream.DataAvailable())
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    var data = await _victronStream.ReadAvailable();

                    var messages = _parser.Parse(data, _parserState);

                    foreach (var message in messages)
                    {
                        switch (message.MessageType)
                        {
                            case VictronMessageType.Text:
                                var textMessage = (VictronTextBlock)message;
                                TextMessageReceived?.Invoke(this, new TextMessageReceivedEventArgs(textMessage));
                                break;
                            case VictronMessageType.Hex:
                                HandleHexMessage((VictronHexMessage)message);
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected Error in Read Thread Worker");
                }
            }
        }

        private void HandleHexMessage(VictronHexMessage message)
        {
            var hexMessage = _hexSerializer.ParseHexMessage(message.Nibbles);
            switch (hexMessage.ResponseType)
            {
                case HexResponse.Get:
                    HandleGetRegisterResponse((GetRegisterResponseMessage)hexMessage);
                    break;
                case HexResponse.Async:
                    HandleAsyncRegisterResponse((AsyncRegisterResponseMessage)hexMessage);
                    break;
                case HexResponse.Ping:
                    HandlePingResponse((PingResponseMessage)hexMessage);
                    break;
                default:
                    throw new ArgumentException($"Unhandled Hex Response Type {hexMessage.ResponseType}");
            }
        }

        private void HandlePingResponse(PingResponseMessage pingResponse)
        {
            if(_pendingPingResponse == null)
            {
                return; // No request found for this response
            }

            _pendingPingResponse.TaskCompletionSource.SetResult(pingResponse.Version);
        }

        private void HandleAsyncRegisterResponse(AsyncRegisterResponseMessage asyncResponse)
        {
            AsyncMessageReceived?.Invoke(this, new AsyncMessageReceivedEventArgs(asyncResponse));
        }

        private void HandleGetRegisterResponse(GetRegisterResponseMessage getMessage)
        {
            var pendingResponse = _pendingGetResponses.FirstOrDefault(x => x.Register == getMessage.Register);
            if(pendingResponse == null)
            {
                return; // No request found for this response
            }

            switch (getMessage.Flags)
            {
                case GetSetResponseFlags.None:
                    pendingResponse.TaskCompletionSource.SetResult(getMessage.RegisterValue);
                    break;
                case GetSetResponseFlags.UnknownId:
                    pendingResponse.TaskCompletionSource.SetException(new UnknownIdException("Unknown Id"));
                    break;
                case GetSetResponseFlags.ParameterError: //Supposed to be only for setting value
                case GetSetResponseFlags.NotSupported: //Supposed to be only for setting value
                default:
                    break;
            }
        }

        public async Task Stop()
        {
            await _startStopLock.WaitAsync();

            try
            {
                _running = false;
                while (_readThread.IsAlive)
                {
                    await Task.Delay(100);
                }
            }
            finally
            {
                _startStopLock.Release();
            }
        }

        public async Task<byte[]> GetRegister(VictronRegister register)
        {
            var command = _hexSerializer.SerializeGetRegister(register);

            var tcs = new TaskCompletionSource<byte[]>();

            var pendingResponse = new PendingGetResponse
            {
                Register = register,
                TaskCompletionSource = tcs
            };

            await _writeLock.WaitAsync();
            try
            {
                _pendingGetResponses.Add(pendingResponse);
                await _victronStream.Write(Encoding.ASCII.GetBytes(command));
            }
            finally
            {
                _writeLock.Release();
            }

            var resultTask = tcs.Task;
            try
            {
                return await WaitWithTimeout(resultTask, _deviceResponseTimeout);
            }
            finally
            {
                _pendingGetResponses.Remove(pendingResponse);
            }
        }

        public async Task SetRegister(VictronRegister register, byte[] data)
        {
            throw new NotImplementedException(); // TODO
        }

        public async Task<byte[]> Ping()
        {
            var command = _hexSerializer.Serialize(HexCommand.Ping, new byte[0]);

            var tcs = new TaskCompletionSource<byte[]>();
            await _writeLock.WaitAsync();
            try
            {
                _pendingPingResponse = new PendingPingResponse
                {
                    TaskCompletionSource = tcs
                };
                await _victronStream.Write(Encoding.ASCII.GetBytes(command));
            }
            finally
            {
                _writeLock.Release();
            }

            try
            {
                return await WaitWithTimeout(tcs.Task, _deviceResponseTimeout);
            }
            finally
            {
                _pendingPingResponse = null;
            }
        }

        private async Task<TResult> WaitWithTimeout<TResult>(Task<TResult> resultTask, int timeout)
        {
            var firstTask = await Task.WhenAny(resultTask, Task.Delay(timeout));

            if (firstTask != resultTask)
            {
                throw new TimeoutException($"No response within {timeout}ms");
            }

            return await resultTask;
        }

        public void Dispose()
        {
            _victronStream.Dispose();
        }

        private class PendingGetResponse
        {
            public VictronRegister Register { get; set; }
            public TaskCompletionSource<byte[]> TaskCompletionSource { get; set; }
        }

        private class PendingPingResponse
        {
            public TaskCompletionSource<byte[]> TaskCompletionSource { get; set; }
        }
    }
}
