using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Models.Report;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericGml;
using Geonorge.XsdValidator.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelper;

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

        public async Task<ValidationReport> Validate(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            var startTime = DateTime.Now;

            var xsdStream = xsdFile?.OpenReadStream() ?? await _xsdHttpClient.GetXsdFromXmlFilesAsync(xmlFiles);
            var xmlMetadata = await XmlMetadata.CreateAsync(xsdStream, _xsdCacheFilesPath);

            using var inputData = GetInputData(xmlFiles);
            var xsdRule = _xsdValidationService.Validate(inputData, xsdStream);
            var rules = new List<Rule> { xsdRule };

            rules.AddRange(await Validate(inputData, xmlMetadata, xsdStream));

            return CreateValidationReport(startTime, xmlMetadata.Namespace, inputData, rules);
        }

        private async Task<List<Rule>> Validate(DisposableList<InputData> inputData, XmlMetadata xmlMetadata, Stream xsdStream)
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
    }
}
