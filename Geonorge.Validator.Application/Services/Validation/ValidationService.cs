using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models.Report;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.GenericGml;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;
using static Geonorge.Validator.Application.Utils.XmlHelpers;

namespace Geonorge.Validator.Application.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IXsdValidationService _xsdValidationService;
        private readonly IXsdHttpClient _xsdHttpClient;
        private readonly IServiceProvider _serviceProvider;
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
            _serviceProvider = httpContextAccessor.HttpContext.RequestServices;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ValidationReport> Validate(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            var startTime = DateTime.Now;

            var xsdStream = xsdFile?.OpenReadStream() ?? await _xsdHttpClient.GetXsdFromXmlFiles(xmlFiles);
            (string xmlNamespace, string xsdVersion) = await GetXmlNamespaceAndXsdVersion(xsdStream);

            using var inputData = GetInputData(xmlFiles);
            var xsdRule = _xsdValidationService.Validate(inputData, xsdStream);
            var rules = new List<Rule> { xsdRule };

            rules.AddRange(await Validate(inputData, xmlNamespace, xsdVersion, xsdStream));

            return CreateValidationReport(startTime, xmlNamespace, inputData, rules);
        }

        private async Task<List<Rule>> Validate(DisposableList<InputData> inputData, string xmlNamespace, string xsdVersion, Stream xsdStream)
        {
            if (inputData.All(data => !data.IsValid))
                return new();

            var validator = GetValidator(xmlNamespace, xsdVersion);

            if (validator != null)
                return await validator.Validate(xmlNamespace, inputData);

            if (InputDataIsGml(inputData))
            {
                var genericGmlValidator = _serviceProvider.GetService(typeof(IGenericGmlValidator)) as IGenericGmlValidator;
                return await genericGmlValidator.Validate(inputData, xsdStream);
            }

            return new();
        }

        private IValidator GetValidator(string xmlNamespace, string xsdVersion)
        {
            var validator = _options.GetValidator(xmlNamespace);

            if (validator == null || !validator.XsdVersions.Contains(xsdVersion))
                return null;

            return _serviceProvider.GetService(validator.ServiceType) as IValidator;
        }

        private static bool InputDataIsGml(DisposableList<InputData> inputData) => inputData.All(data => Path.GetExtension(data.FileName) == ".gml");
    }
}
