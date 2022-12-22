using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.XmlSchema;
using Geonorge.Validator.Application.HttpClients.XmlSchemaCacher;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.XmlSchemaValidation;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericGml;
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
        private readonly IXmlSchemaCacherHttpClient _xmlSchemaCacherHttpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<XmlValidationService> _logger;

        public XmlValidationService(
            IXmlSchemaValidationService xmlSchemaValidationService,
            IXmlSchemaHttpClient xmlSchemaHttpClient,
            IXmlSchemaCacherHttpClient xmlSchemaCacherHttpClient,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            ILogger<XmlValidationService> logger)
        {
            _xmlSchemaValidationService = xmlSchemaValidationService;
            _xmlSchemaHttpClient = xmlSchemaHttpClient;
            _xmlSchemaCacherHttpClient = xmlSchemaCacherHttpClient;
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _validatorOptions = validatorOptions.Value;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ValidationReport> ValidateAsync(Submittal submittal)
        {
            var startTime = DateTime.Now;

            var xmlSchemaData = await GetXmlSchemaDataAsync(submittal);
            await CacheXmlSchemasAsync(xmlSchemaData.SchemaUris);
            var xmlMetadata = await XmlMetadata.CreateAsync(submittal.InputData.First().Stream, xmlSchemaData.Streams);
            
            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");

            var namespaces = xmlMetadata.Namespaces.Select(tuple => tuple.Namespace).ToList();
            var xmlSchemaValidationResult = await _xmlSchemaValidationService.ValidateAsync(submittal.InputData, xmlSchemaData, namespaces);
            
            var rules = new List<Rule> { xmlSchemaValidationResult.Rule };

            rules.AddRange(await ValidateAsync(submittal.InputData, xmlMetadata, xmlSchemaValidationResult, submittal.SkipRules));

            var report = ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, submittal.InputData, namespaces, startTime);
            submittal.InputData.Dispose();

            return report;
        }

        private async Task<List<Rule>> ValidateAsync(
            DisposableList<InputData> inputData, XmlMetadata xmlMetadata, XmlSchemaValidationResult xmlSchemaValidationResult, List<string> skipRules)
        {
            if (inputData.All(data => !data.IsValid))
                return new();

            var validators = xmlMetadata.Namespaces
                .Select(@namespace => (@namespace.Namespace, Validator: GetValidator(@namespace.Namespace, @namespace.XsdVersion)))
                .Where(tuple => tuple.Validator != null)
                .ToList();

            if (validators.Any())
            {
                var rules = new List<Rule>();

                foreach (var (@namespace, validator) in validators)
                    rules.AddRange(await validator.Validate(@namespace, inputData, skipRules));

                return rules;
            }

            if (!xmlMetadata.IsGml32)
                return new();

            var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGmlValidator)) as IGenericGmlValidator;

            return await genericGmlValidator.Validate(
                inputData, 
                xmlSchemaValidationResult.XmlSchemaElements,
                xmlSchemaValidationResult.XmlSchemaSet, 
                skipRules
            );
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
                var stream = await _xmlSchemaHttpClient.FetchXmlSchemaAsync(submittal.SchemaUri);
                var xmlSchemaData = new XmlSchemaData();
                xmlSchemaData.Streams.Add(stream);
                xmlSchemaData.SchemaUris.Add(submittal.SchemaUri);

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

        private async Task CacheXmlSchemasAsync(List<Uri> schemaUris)
        {
            foreach (var schemaUri in schemaUris)
                await _xmlSchemaCacherHttpClient.CacheSchemasAsync(schemaUri);
        }
    }
}
