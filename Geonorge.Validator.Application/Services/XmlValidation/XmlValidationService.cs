using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.XmlSchema;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericGml;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XmlValidation
{
    public class XmlValidationService : IXmlValidationService
    {
        private readonly IXmlSchemaValidationService _xsdValidationService;
        private readonly IXmlSchemaHttpClient _xsdHttpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<XmlValidationService> _logger;
        private readonly string _xsdCacheFilesPath;

        public XmlValidationService(
            IXmlSchemaValidationService xsdValidationService,
            IXmlSchemaHttpClient xsdHttpClient,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            IOptions<XsdValidatorSettings> xsdValidatorOptions,
            ILogger<XmlValidationService> logger)
        {
            _xsdValidationService = xsdValidationService;
            _xsdHttpClient = xsdHttpClient;
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _validatorOptions = validatorOptions.Value;
            _xsdCacheFilesPath = xsdValidatorOptions.Value.CacheFilesPath;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ValidationReport> ValidateAsync(Submittal submittal)
        {
            var startTime = DateTime.Now;
            var xmlSchemaData = await GetXmlSchemaDataAsync(submittal.InputData, submittal.Schema);
            
            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");

            var xsdValidationResult = await _xsdValidationService.ValidateAsync(submittal.InputData, xmlSchemaData);
            var xmlMetadata = await XmlMetadata.CreateAsync(xmlSchemaData.Streams[0], _xsdCacheFilesPath);

            var rules = new List<Rule> { xsdValidationResult.Rule };
            rules.AddRange(await ValidateAsync(submittal.InputData, xmlMetadata, xsdValidationResult.CodelistUris, submittal.SkipRules));

            var report = ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, submittal.InputData, xmlMetadata.Namespace, startTime);
            submittal.InputData.Dispose();

            return report;
        }

        private async Task<List<Rule>> ValidateAsync(
            DisposableList<InputData> inputData, XmlMetadata xmlMetadata, Dictionary<string, Uri> codelistUris, List<string> skipRules)
        {
            if (inputData.All(data => !data.IsValid))
                return new();
            
            var validator = GetValidator(xmlMetadata.Namespace, xmlMetadata.XsdVersion);

            if (validator != null)
                return await validator.Validate(xmlMetadata.Namespace, inputData, skipRules);

            if (!xmlMetadata.IsGml32)
                return new();

            var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGmlValidator)) as IGenericGmlValidator;

            return await genericGmlValidator.Validate(inputData, codelistUris, skipRules);
        }

        private IXmlValidator GetValidator(string xmlNamespace, string xsdVersion)
        {
            var validator = _validatorOptions.GetValidator(xmlNamespace);

            if (validator == null || !validator.XsdVersions.Contains(xsdVersion))
                return null;

            return _serviceProvider.GetService(validator.ServiceType) as IXmlValidator;
        }

        private async Task<XmlSchemaData> GetXmlSchemaDataAsync(DisposableList<InputData> files, Stream schema)
        {
            if (schema == null)
                return await _xsdHttpClient.GetXmlSchemaFromInputDataAsync(files);

            var xsdData = new XmlSchemaData();
            xsdData.Streams.Add(schema);

            return xsdData;
        }
    }
}
