using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Cache
{
    public class CacheService : BackgroundService
    {
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly IXsdHttpClient _xsdHttpClient;
        private readonly TimeOnly _timeOfUpdate;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            ICodelistHttpClient codelistHttpClient,
            IXsdHttpClient xsdHttpClient,
            IOptions<CacheSettings> options,
            ILogger<CacheService> logger)
        {
            _codelistHttpClient = codelistHttpClient;
            _xsdHttpClient = xsdHttpClient;
            _timeOfUpdate = TimeOnly.Parse(options.Value.TimeOfUpdate);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await Task.Delay(GetTimeUntilNextTask(), stoppingToken);

                var count1 = await _codelistHttpClient.UpdateCacheAsync();
                _logger.LogInformation("Oppdaterer cache for kodelister: {0} filer ble oppdatert", count1);

                var count2 = await _xsdHttpClient.UpdateCacheAsync();
                _logger.LogInformation("Oppdaterer cache for XML-skjemaer: {0} filer ble oppdatert", count2);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private TimeSpan GetTimeUntilNextTask()
        {
            var currentTimeOfDay = DateTime.Now.TimeOfDay;
            var timeUntilNextTask = _timeOfUpdate.ToTimeSpan().Subtract(currentTimeOfDay);

            if (timeUntilNextTask <= TimeSpan.Zero)
                timeUntilNextTask += TimeSpan.FromHours(24);

            return timeUntilNextTask;
        }
    }
}
