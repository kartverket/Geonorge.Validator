using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models.Data;
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
using System.IO;
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
        private readonly string _xsdCacheFilesPath;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IXsdValidationService xsdValidationService,
            IXsdHttpClient xsdHttpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ValidatorOptions> validatorOptions,
            IOptions<XsdValidatorSettings> xsdValidatorOptions,
            ILogger<ValidationService> logger)
        {
            _xsdValidationService = xsdValidationService;
            _xsdHttpClient = xsdHttpClient;
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _validatorOptions = validatorOptions.Value;
            _xsdCacheFilesPath = xsdValidatorOptions.Value.CacheFilesPath;
            _logger = logger;
        }

        public async Task<ValidationReport> ValidateAsync(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            var startTime = DateTime.Now;

            var xsdData = await GetXsdDataAsync(xmlFiles, xsdFile);
            using var inputData = GetInputData(xmlFiles);

            var xsdRule = _xsdValidationService.Validate(inputData, xsdData);
            var xmlMetadata = await XmlMetadata.CreateAsync(xsdData.Stream, _xsdCacheFilesPath);

            var rules = new List<Rule> { xsdRule };
            rules.AddRange(await ValidateAsync(inputData, xmlMetadata, xsdData.Stream));

            return ValidationReport.Create(ContextCorrelator.GetValue("CorrelationId"), rules, inputData, xmlMetadata.Namespace, startTime);
        }

        private async Task<List<Rule>> ValidateAsync(DisposableList<InputData> inputData, XmlMetadata xmlMetadata, Stream xsdStream)
        {
            if (inputData.All(data => !data.IsValid))
                return new();

            var validator = GetValidator(xmlMetadata.Namespace, xmlMetadata.XsdVersion);

            if (validator != null)
                return await validator.Validate(xmlMetadata.Namespace, inputData);

            if (!xmlMetadata.IsGml32)
                return new();

            var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGmlValidator)) as IGenericGmlValidator;

            return await genericGmlValidator.Validate(inputData, xsdStream);
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

            return new XsdData { Stream = xsdFile.OpenReadStream() };
        }

        private static DisposableList<InputData> GetInputData(List<IFormFile> files)
        {
            return files
                .Select(file => new InputData(file.OpenReadStream(), file.FileName, null))
                .ToDisposableList();
        }
    }
}
