using DiBK.RuleValidator;
using DiBK.RuleValidator.Config;
using Geonorge.Validator.Application.Models.Report;
using Geonorge.Validator.Application.Services.Validators.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

namespace Geonorge.Validator.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IXsdValidationService _xsdValidationService;
        private readonly IValidatorService _validatorService;
        private readonly ValidatorOptions _options;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IXsdValidationService xsdValidationService,
            IValidatorService validatorService,
            IOptions<ValidatorOptions> options,
            ILogger<ValidationService> logger)
        {
            _xsdValidationService = xsdValidationService;
            _validatorService = validatorService;
            _options = options.Value;
            _logger = logger;
        }

        public ValidationReport Validate(List<IFormFile> files, string xmlNamespace)
        {
            var startTime = DateTime.Now;
            var allowedFileTypes = _options.GetAllowedFileTypes(xmlNamespace);

            using var inputData = GetInputData(files, allowedFileTypes);
            var schemaRule = _xsdValidationService.Validate(inputData, xmlNamespace);
            var rules = new List<Rule> { schemaRule };

            var validator = _validatorService.GetValidator(xmlNamespace);
            
            rules.AddRange(validator.Validate(xmlNamespace, inputData));

            return CreateValidationReport(startTime, inputData, rules);
        }
    }
}
