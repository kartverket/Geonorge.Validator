using Geonorge.Validator.Application.HttpClients.Codelist;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Cache
{
    public class CacheService : BackgroundService
    {
        private static readonly TimeOnly TimeOfTask = new(3, 0);

        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            ICodelistHttpClient codelistHttpClient,
            ILogger<CacheService> logger)
        {
            _codelistHttpClient = codelistHttpClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await Task.Delay(GetTimeUntilNextTask(), stoppingToken);
                
                await _codelistHttpClient.UpdateCacheAsync();
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private static TimeSpan GetTimeUntilNextTask()
        {
            var currentTimeOfDay = DateTime.Now.TimeOfDay;
            var timeUntilNextTask = TimeOfTask.ToTimeSpan().Subtract(currentTimeOfDay);

            if (timeUntilNextTask <= TimeSpan.Zero)
                timeUntilNextTask += TimeSpan.FromHours(24);

            return timeUntilNextTask;
        }
    }
}
