using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VictronDataAdapter.Contracts;

namespace VictronDataAdapter
{
    class Host : IHostedService
    {
        private readonly IVictronStreamAdapter streamAdapter;
        private readonly IVictronDataSource dataSource;
        private readonly CancellationTokenSource cts;
        private IDataReader reader;

        public Host(IVictronStreamAdapter streamAdapter, IVictronDataSource dataSource)
        {
            this.streamAdapter = streamAdapter;
            this.dataSource = dataSource;
            this.cts = new CancellationTokenSource();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.reader = this.dataSource.GetDataReader();

            Task.Run(() => Run());

            return Task.CompletedTask;
        }

        public async Task Run()
        {
            while (!this.cts.IsCancellationRequested)
            {
                var point = await this.streamAdapter.GetNextDataPoint(this.reader);

                //TODO: send
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
