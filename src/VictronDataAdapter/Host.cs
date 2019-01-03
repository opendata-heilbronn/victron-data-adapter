using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;
using VictronDataAdapter.Impl;

namespace VictronDataAdapter
{
    internal class Host : IHostedService
    {
        private readonly IVictronStreamAdapter streamAdapter;
        private readonly IVictronDataSource dataSource;
        private readonly ILogger<Host> logger;
        private readonly InfluxDbConfiguration influxConfig;
        private readonly CancellationTokenSource cts;
        private IDataReader reader;
        private InfluxDbClient writer;

        public Host(IVictronStreamAdapter streamAdapter, IVictronDataSource dataSource, ILogger<Host> logger, IOptions<InfluxDbConfiguration> influxConfig)
        {
            this.streamAdapter = streamAdapter;
            this.dataSource = dataSource;
            this.logger = logger;
            this.influxConfig = influxConfig.Value;
            this.cts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.reader = this.dataSource.GetDataReader();
            this.writer = new InfluxDbClient(this.influxConfig.Endpoint, this.influxConfig.Username, this.influxConfig.Password, InfluxDbVersion.Latest);

            Task.Run(() => Run());

            return Task.CompletedTask;
        }

        public async Task Run()
        {
            while (!this.cts.IsCancellationRequested)
            {
                Point point = null;

                try
                {
                    point = await this.streamAdapter.GetNextDataPoint(this.reader);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error while getting data point!");
                }

                try
                {
                    if (point != null)
                    {
                        point.Name = this.influxConfig.Measurement;
                        await this.writer.Client.WriteAsync(point, this.influxConfig.Database);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error while sending data point!");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.reader?.Dispose();
            this.dataSource?.Dispose();
            return Task.CompletedTask;
        }
    }
}
