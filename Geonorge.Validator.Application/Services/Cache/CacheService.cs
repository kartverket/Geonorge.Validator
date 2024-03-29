﻿using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.HttpClients.XmlSchemaCacher;
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
        private readonly IXmlSchemaCacherHttpClient _xmlSchemaCacherHttpClient;
        private readonly IJsonSchemaHttpClient _jsonSchemaHttpClient;
        private readonly TimeOnly _timeOfUpdate;
        private readonly ILogger<CacheService> _logger;

        public CacheService(
            ICodelistHttpClient codelistHttpClient,
            IXmlSchemaCacherHttpClient xmlSchemaCacherHttpClient,
            IJsonSchemaHttpClient jsonSchemaHttpClient,
            IOptions<CacheSettings> options,
            ILogger<CacheService> logger)
        {
            _codelistHttpClient = codelistHttpClient;
            _xmlSchemaCacherHttpClient = xmlSchemaCacherHttpClient;
            _jsonSchemaHttpClient = jsonSchemaHttpClient;
            _timeOfUpdate = TimeOnly.Parse(options.Value.TimeOfUpdate);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                await Task.Delay(GetTimeUntilNextTask(), stoppingToken);

                var count1 = await _codelistHttpClient.UpdateCacheAsync();
                _logger.LogInformation("Oppdaterer cache for kodelister: {count1} filer ble oppdatert", count1);

                var count2 = await _xmlSchemaCacherHttpClient.UpdateCacheAsync();
                _logger.LogInformation("Oppdaterer cache for XML-skjemaer: {count2} filer ble oppdatert", count2);

                var count3 = await _jsonSchemaHttpClient.UpdateCacheAsync();
                _logger.LogInformation("Oppdaterer cache for JSON-skjemaer: {count3} filer ble oppdatert", count3);
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
