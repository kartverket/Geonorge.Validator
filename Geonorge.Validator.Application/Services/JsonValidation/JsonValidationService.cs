using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericJson;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.JsonValidation
{
    public class JsonValidationService : IJsonValidationService
    {
        private readonly IJsonSchemaHttpClient _jsonSchemaHttpClient;
        private readonly IJsonSchemaValidationService _jsonSchemaValidationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<JsonValidationService> _logger;

        public JsonValidationService(
            IJsonSchemaHttpClient jsonSchemaHttpClient,
            IJsonSchemaValidationService jsonSchemaValidationService,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            ILogger<JsonValidationService> logger)
        {
            _jsonSchemaHttpClient = jsonSchemaHttpClient;
            _jsonSchemaValidationService = jsonSchemaValidationService;
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _validatorOptions = validatorOptions.Value;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ValidationReport> ValidateAsync(Submittal submittal)
        {
            var startTime = DateTime.Now;
            var schema = await _jsonSchemaHttpClient.GetJsonSchemaAsync(submittal.InputData, submittal.Schema);

            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");
            var jsonSchemaValidationResult = await _jsonSchemaValidationService.ValidateAsync(submittal.InputData, schema);

            var rules = new List<Rule> { jsonSchemaValidationResult.Rule };

            rules.AddRange(await ValidateAsync(schema, submittal.InputData, submittal.SkipRules, jsonSchemaValidationResult.GeoJsonFiles));

            var report = ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, submittal.InputData, new List<string>(), startTime);
            submittal.InputData.Dispose();

            return report;
        }

        private async Task<List<Rule>> ValidateAsync(JSchema schema, DisposableList<InputData> inputData, List<string> skipRules, List<string> geoJsonFiles)
        {
            if (inputData.All(data => !data.IsValid))
                return new();

            var schemaId = schema.Id?.ToString();
            var validator = GetValidator(schemaId);

            if (validator != null)
                return await validator.ValidateAsync(schemaId, inputData, skipRules);

            var geoJsonInputData = inputData
                .Where(data => geoJsonFiles.Contains(data.FileName))
                .ToDisposableList();

            if (!geoJsonInputData.Any())
                return new();

            var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGeoJsonValidator)) as IGenericGeoJsonValidator;

            return await genericGmlValidator.ValidateAsync(null, geoJsonInputData, skipRules);
        }

        private IJsonValidator GetValidator(string schemaId)
        {
            if (schemaId == null)
                return null;

            var validator = _validatorOptions.GetValidator(schemaId);

            if (validator == null)
                return null;

            return _serviceProvider.GetService(validator.ServiceType) as IJsonValidator;
        }
    }
}
