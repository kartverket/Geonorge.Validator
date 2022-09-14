using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Models.Data.Json;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericJson;
using Geonorge.Validator.GeoJson.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.JsonValidation
{
    public class JsonValidationService : IJsonValidationService
    {
        private readonly IRuleValidator _validator;
        private readonly IJsonSchemaHttpClient _jsonSchemaHttpClient;
        private readonly IJsonSchemaValidationService _jsonSchemaValidationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<JsonValidationService> _logger;

        public JsonValidationService(
            IRuleValidator validator,
            IJsonSchemaHttpClient jsonSchemaHttpClient,
            IJsonSchemaValidationService jsonSchemaValidationService,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            ILogger<JsonValidationService> logger)
        {
            _validator = validator;
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

            var schemaValidationInput = JsonSchemaValidationInput.Create(
                submittal.InputData,
                inputData => _jsonSchemaValidationService.Validate(inputData, schema)
            );

            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");
            await _validator.Validate(schemaValidationInput, options => { });

            var rules = _validator.GetAllRules();
            rules.AddRange(await ValidateAsync(schema, submittal.InputData, submittal.SkipRules));

            var report = ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, submittal.InputData, null, startTime);
            submittal.InputData.Dispose();

            return report;
        }

        private async Task<List<Rule>> ValidateAsync(JSchema schema, DisposableList<InputData> inputData, List<string> skipRules)
        {
            if (inputData.All(data => !data.IsValid) || schema.Id == null)
                return new();

            var schemaId = schema.Id.ToString();
            var validator = GetValidator(schemaId);

            if (validator != null)
                return await validator.ValidateAsync(schemaId, inputData, skipRules);

            if (!GeoJsonHelper.HasGeoJson(schema))
                return new();

            var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGeoJsonValidator)) as IGenericGeoJsonValidator;

            return await genericGmlValidator.ValidateAsync(schemaId, inputData, skipRules);
        }

        private IJsonValidator GetValidator(string schemaId)
        {
            var validator = _validatorOptions.GetValidator(schemaId);

            if (validator == null)
                return null;

            return _serviceProvider.GetService(validator.ServiceType) as IJsonValidator;
        }
    }
}
