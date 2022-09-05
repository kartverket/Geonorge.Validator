using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericGml;
using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IXsdValidationService _xsdValidationService;
        private readonly IXsdHttpClient _xsdHttpClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ValidatorOptions _validatorOptions;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ValidationService> _logger;
        private readonly string _xsdCacheFilesPath;

        public ValidationService(
            IXsdValidationService xsdValidationService,
            IXsdHttpClient xsdHttpClient,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService,
            IOptions<ValidatorOptions> validatorOptions,
            IOptions<XsdValidatorSettings> xsdValidatorOptions,
            ILogger<ValidationService> logger)
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

            var xsdData = await GetXsdDataAsync(submittal.Files, submittal.Schema);
            using var inputData = GetInputData(submittal.Files);

            await _notificationService.SendAsync("Validerer mot applikasjonsskjema");

            var xsdValidationResult = _xsdValidationService.Validate(inputData, xsdData);
            var xmlMetadata = await XmlMetadata.CreateAsync(xsdData.Streams[0], _xsdCacheFilesPath);

            var rules = new List<Rule> { xsdValidationResult.Rule };
            rules.AddRange(await ValidateAsync(inputData, xmlMetadata, xsdValidationResult.CodelistUris, submittal.SkipRules));

            return ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, inputData, xmlMetadata.Namespace, startTime);
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

        private IValidator GetValidator(string xmlNamespace, string xsdVersion)
        {
            var validator = _validatorOptions.GetValidator(xmlNamespace);

            if (validator == null || !validator.XsdVersions.Contains(xsdVersion))
                return null;

            return _serviceProvider.GetService(validator.ServiceType) as IValidator;
        }

        private async Task<XsdData> GetXsdDataAsync(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            if (xsdFile == null)
                return await _xsdHttpClient.GetXsdFromXmlFilesAsync(xmlFiles);

            var xsdData = new XsdData();
            xsdData.Streams.Add(xsdFile.OpenReadStream());

            return xsdData;
        }

        private static DisposableList<InputData> GetInputData(List<IFormFile> files)
        {
            return files
                .Select(file => new InputData(file.OpenReadStream(), file.FileName, null))
                .ToDisposableList();
        }
    }
}
