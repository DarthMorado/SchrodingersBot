using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public abstract class BotDaemon<T> : IHostedService
        where T : BotOptions
    {
        private CancellationTokenSource _cancellationTokenSource;
        public bool Enabled { get; set; }
        public TimeSpan Interval { get; set; } = TimeSpan.FromMinutes(1);
        private List<long> SubscribedChats { get; set; }
        private DateTime LastRun { get; set; }

        public abstract Task<List<Answer>> Action();
        public abstract Task Subscribe(long chatId, Dictionary<string, object> options);
        public abstract Task UnSubscribe(long chatId, Dictionary<string, object> options);

        protected BotDaemon()
        {
            _cancellationTokenSource = new();
        }

        public async Task ChangeInterval(TimeSpan interval)
        {
            Interval = interval;
        }

        public async Task ChangeEnabledStatus(bool enabled)
        {
            Enabled = enabled;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Action();
                await Task.Delay(Interval);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Run(_cancellationTokenSource.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
