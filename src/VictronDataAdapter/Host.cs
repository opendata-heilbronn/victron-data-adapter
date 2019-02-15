using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Impl;

namespace VictronDataAdapter
{
    internal class Host : IHostedService
    {
        private readonly IVictronStreamAdapter _streamAdapter;
        private readonly IVictronDataReaderFactory _dataSource;
        private readonly ILogger<Host> _logger;
        private readonly InfluxDbConfiguration _influxConfig;
        private readonly CancellationTokenSource _cts;
        private IDataReader _reader;
        private InfluxDbClient _writer;
        private ConcurrentQueue<Point> _sendQueue;

        public Host(IVictronStreamAdapter streamAdapter, IVictronDataReaderFactory dataSource, ILogger<Host> logger, IOptions<InfluxDbConfiguration> influxConfig)
        {
            _streamAdapter = streamAdapter;
            _dataSource = dataSource;
            _logger = logger;
            _influxConfig = influxConfig.Value;
            _sendQueue = new ConcurrentQueue<Point>();
            _cts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _reader = _dataSource.GetDataReader();
            _writer = new InfluxDbClient(_influxConfig.Endpoint, _influxConfig.Username, _influxConfig.Password, InfluxDbVersion.Latest);

            Task.Run(() => CollectData());
            Task.Run(() => SendData());

            return Task.CompletedTask;
        }

        public async Task CollectData()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var point = await _streamAdapter.GetNextDataPoint(_reader);
                    if (point != null)
                    {
                        _sendQueue.Enqueue(point);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while getting data point!");
                }
            }
        }
        public async Task SendData()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (!_sendQueue.TryDequeue(out var point))
                {
                    await Task.Delay(500);
                    continue;
                }

                try
                {
                    point.Name = _influxConfig.Measurement;
                    await _writer.Client.WriteAsync(point, _influxConfig.Database);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending data point!");
                    if (_sendQueue.Count <= 1000)
                    {
                        _sendQueue.Enqueue(point);
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _reader?.Dispose();
            return Task.CompletedTask;
        }
    }
}
