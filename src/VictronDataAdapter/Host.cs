using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VeDirectCommunication;

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
        private IVictronStream _reader;
        private InfluxDbClient _writer;
        private ConcurrentQueue<Point> _sendQueue;

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
            _device.TextMessageReceived += TextMessageReceived;
            _device.AsyncMessageReceived += (sender, e) => _logger.LogInformation($"{e.Data.Register}: {e.Data.RegisterValue}");

            var version = await _device.Ping();
            //_writer = new InfluxDbClient(_influxConfig.Endpoint, _influxConfig.Username, _influxConfig.Password, InfluxDbVersion.Latest);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(() => Ping());
            Task.Run(() => SendData());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void TextMessageReceived(object sender, TextMessageReceivedEventArgs args)
        {
            _logger.LogInformation("Got {MessageCount} Messages in Packet", args.Data.Fields.Count);
            var point = _streamAdapter.GetNextDataPoint(args.Data);
            if (point != null)
            {
                _sendQueue.Enqueue(point);
            }
        }

        public async Task Ping()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await _device.Ping();
                    await Task.Delay(30 * 1000, _cts.Token); //every 30 seconds
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while pinging!");
                }
            }
        }

        public async Task SendData()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (!_sendQueue.TryDequeue(out var point))
                {
                    await Task.Delay(500, _cts.Token);
                    continue;
                }

                try
                {
                    point.Name = _influxConfig.Measurement;

                    //TODO: disabled for debugging
                    //await _writer.Client.WriteAsync(point, _influxConfig.Database);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending data point!");
                    if (_sendQueue.Count <= 1000)
                    {
                        _sendQueue.Enqueue(point);
                        _logger.LogInformation("Retrying...");
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _device.Stop();
            _reader?.Dispose();
            return Task.CompletedTask;
        }
    }
}
