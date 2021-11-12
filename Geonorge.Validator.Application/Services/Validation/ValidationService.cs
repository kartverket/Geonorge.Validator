using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models.Report;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

namespace Geonorge.Validator.Application.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IXsdValidationService _xsdValidationService;
        private readonly IXsdHttpClient _xsdHttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ValidatorOptions _options;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IXsdValidationService xsdValidationService,
            IXsdHttpClient xsdHttpClient,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ValidatorOptions> options,
            ILogger<ValidationService> logger)
        {
            _xsdValidationService = xsdValidationService;
            _xsdHttpClient = xsdHttpClient;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ValidationReport> Validate(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            var startTime = DateTime.Now;
            (string xmlNamespace, string xsdVersion) = await _xsdHttpClient.GetXmlNamespaceAndXsdVersion(xmlFiles, xsdFile);
            var allowedFileTypes = _options.GetAllowedFileTypes(xmlNamespace);

            using var inputData = GetInputData(xmlFiles, allowedFileTypes);
            var schemaRule = _xsdValidationService.Validate(inputData, xsdFile?.OpenReadStream());
            var rules = new List<Rule> { schemaRule };

            rules.AddRange(await Validate(xmlNamespace, xsdVersion, inputData));

            return CreateValidationReport(startTime, xmlNamespace, inputData, rules);
        }

        private async Task<List<Rule>> Validate(string xmlNamespace, string xsdVersion, DisposableList<InputData> inputData)
        {
            if (inputData.All(data => !data.IsValid))
                return new();

            var validator = GetValidator(xmlNamespace, xsdVersion);

            if (validator == null)
                return new();

            return await validator.Validate(xmlNamespace, inputData);
        }

        private IValidator GetValidator(string xmlNamespace, string xsdVersion)
        {
            var validator = _options.GetValidator(xmlNamespace);

            if (validator == null || !validator.XsdVersions.Contains(xsdVersion))
                return null;

            return _httpContextAccessor.HttpContext.RequestServices.GetService(validator.ServiceType) as IValidator;
        }
    }
}
