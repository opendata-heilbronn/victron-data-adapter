using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VeDirectCommunication;
using VeDirectCommunication.HexMode;

namespace VictronDataAdapter
{
    internal class Host : IHostedService
    {
        private readonly IVictronStreamAdapter _streamAdapter;
        private readonly IVeDirectDevice _device;
        private readonly IOptions<IpDataSourceConfig> _dataSourceConfig;
        private readonly ILogger<Host> _logger;
        private readonly InfluxDbConfiguration _influxConfig;
        private readonly CancellationTokenSource _cts;
        private InfluxDbClient _writer;
        private ConcurrentQueue<Point> _sendQueue;

        private readonly IDictionary<VictronRegister, byte[]> _currentStats = new ConcurrentDictionary<VictronRegister, byte[]>();
        private string _serialNumber = string.Empty;
        private List<VictronRegister> _asyncRegisters;
        private readonly VictronRegister[] _statsRegisters = new VictronRegister[]
        {
            VictronRegister.ChargerVoltage,
            VictronRegister.ChargerCurrent,
            VictronRegister.PanelVoltage,
            VictronRegister.PanelPower,
            VictronRegister.DeviceState,
            VictronRegister.ChargerErrorCode,
            VictronRegister.LoadOutputState,
            VictronRegister.LoadOutputVoltage,
            VictronRegister.LoadCurrent,
            VictronRegister.SystemYield,
            VictronRegister.ChargerTemperature,
            VictronRegister.DeviceOffReason
        };

        public Host(IVictronStreamAdapter streamAdapter, IVeDirectDevice device, IOptions<IpDataSourceConfig> dataSourceConfig, ILogger<Host> logger, IOptions<InfluxDbConfiguration> influxConfig)
        {
            _streamAdapter = streamAdapter;
            _device = device;
            _dataSourceConfig = dataSourceConfig;
            _logger = logger;
            _influxConfig = influxConfig.Value;
            _sendQueue = new ConcurrentQueue<Point>();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _device.Start();
            _logger.LogInformation("Initializing Stats...");
            await InitStats();
            _device.AsyncMessageReceived += AsyncReceived;

            _logger.LogInformation("Getting Device version...");
            var version = await _device.Ping();
            var asyncRegisters = SupportedAsyncRegisters.Get(version).ToList();
            _asyncRegisters = asyncRegisters;

            _writer = new InfluxDbClient(_influxConfig.Endpoint, _influxConfig.Username, _influxConfig.Password, InfluxDbVersion.Latest);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => GetNonAsync(asyncRegisters));
            Task.Run(() => SendData());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            _logger.LogInformation("Start completed.");
        }

        private async Task InitStats()
        {
            var tasks = _statsRegisters.Concat(new[] { VictronRegister.SerialNumber, VictronRegister.ProductId })
                .Select(register =>
                    _device.GetRegister(register)
                        .ContinueWith(byteTask => _currentStats[register] = byteTask.Result));

            await Task.WhenAll(tasks);
            AddCurrentToQueue();
        }

        private void AsyncReceived(object sender, AsyncMessageReceivedEventArgs e)
        {
            if (!_asyncRegisters.Contains(e.Data.Register))
            {
                _logger.LogWarning($"Unexpected Async Register {e.Data.Register}");
                return;
            }

            if (!_statsRegisters.Contains(e.Data.Register))
            {
                return;
            }

            _currentStats[e.Data.Register] = e.Data.RegisterValue;
            AddCurrentToQueue();
        }

        public async Task GetNonAsync(IList<VictronRegister> asyncRegisters)
        {
            var nonAsync = _statsRegisters.Except(asyncRegisters);

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var tasks = nonAsync.Select(register =>
                        _device.GetRegister(register)
                            .ContinueWith(byteTask => _currentStats[register] = byteTask.Result));

                    await Task.WhenAll(tasks);
                    AddCurrentToQueue();
                    await Task.Delay(30 * 1000, _cts.Token); //every 10 seconds
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while getting non-async data!");
                }
            }
        }

        private void AddCurrentToQueue()
        {
            var point = _streamAdapter.GetNextDataPoint(_currentStats);
            _sendQueue.Enqueue(point);
        }

        public async Task SendData()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(1000, _cts.Token);

                var toSend = new List<Point>();

                while (_sendQueue.TryDequeue(out var point))
                {
                    point.Name = _influxConfig.Measurement;
                    toSend.Add(point);
                }

                if (toSend.Count == 0)
                    continue;

                try
                {
                    await _writer.Client.WriteAsync(toSend, _influxConfig.Database);

                    _logger.LogInformation("Sent {Count} data points...", toSend.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending data points!");
                    if (_sendQueue.Count <= 1000)
                    {
                        foreach (var point in toSend)
                        {
                            _sendQueue.Enqueue(point);
                        }
                        _logger.LogInformation($"Retrying... ({_sendQueue.Count} Items in queue)");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _device.Stop();
            return Task.CompletedTask;
        }
    }
}
