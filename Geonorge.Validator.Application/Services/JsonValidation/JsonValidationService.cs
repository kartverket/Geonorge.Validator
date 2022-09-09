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

            using var inputData = GetInputData(submittal.Files);
            var schema = await _jsonSchemaHttpClient.GetJsonSchemaAsync(submittal.Files, submittal.Schema);

            var schemaValidationInput = JsonSchemaValidationInput.Create(
                inputData,
                inputData => _jsonSchemaValidationService.Validate(inputData, schema)
            );

            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");
            await _validator.Validate(schemaValidationInput, options => { });

            var rules = _validator.GetAllRules();
            rules.AddRange(await ValidateAsync(schema, inputData, submittal.SkipRules));

            return ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, inputData, null, startTime);
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

        private static DisposableList<InputData> GetInputData(List<IFormFile> files)
        {
            return files
                .Select(file =>
                {
                    var ms = new MemoryStream();
                    file.OpenReadStream().CopyTo(ms);
                    ms.Position = 0;

                    return new InputData(ms, file.FileName, null);
                })
                .ToDisposableList();
        }
    }
}
