using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.XmlSchema;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.XmlSchemaValidation;
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
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XmlValidation
{
    public class XmlValidationService : IXmlValidationService
    {
        private readonly IXmlSchemaValidationService _xmlSchemaValidationService;
        private readonly IXmlSchemaHttpClient _xmlSchemaHttpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<XmlValidationService> _logger;
        private readonly string _xmlSchemaCacheFilesPath;

        public XmlValidationService(
            IXmlSchemaValidationService xmlSchemaValidationService,
            IXmlSchemaHttpClient xmlSchemaHttpClient,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            IOptions<XmlSchemaValidatorSettings> xmlSchemaValidatorSettings,
            ILogger<XmlValidationService> logger)
        {
            _xmlSchemaValidationService = xmlSchemaValidationService;
            _xmlSchemaHttpClient = xmlSchemaHttpClient;
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _validatorOptions = validatorOptions.Value;
            _xmlSchemaCacheFilesPath = xmlSchemaValidatorSettings.Value.CacheFilesPath;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ValidationReport> ValidateAsync(Submittal submittal)
        {
            var startTime = DateTime.Now;

            var xmlSchemaData = await GetXmlSchemaDataAsync(submittal);
            var xmlMetadata = await XmlMetadata.CreateAsync(xmlSchemaData.Streams[0], _xmlSchemaCacheFilesPath);

            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");

            var xsdValidationResult = await _xmlSchemaValidationService.ValidateAsync(submittal.InputData, xmlSchemaData, xmlMetadata.Namespace);
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

        private async Task<XmlSchemaData> GetXmlSchemaDataAsync(Submittal submittal)
        {
            if (submittal.SchemaUri != null)
            {
                var stream = await _xmlSchemaHttpClient.FetchXmlSchemaAsync(submittal.SchemaUri.AbsoluteUri);
                var xmlSchemaData = new XmlSchemaData();
                xmlSchemaData.Streams.Add(stream);

                return xmlSchemaData;
            }
            else if (submittal.Schema != null)
            {
                var xmlSchemaData = new XmlSchemaData();
                xmlSchemaData.Streams.Add(submittal.Schema);

                return xmlSchemaData;
            }
            else
            {
                return await _xmlSchemaHttpClient.GetXmlSchemaFromInputDataAsync(submittal.InputData);
            }
        }
    }
}
