using DiBK.RuleValidator;
using Geonorge.Validator.Application.Models.Report;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

namespace Geonorge.Validator.Application.Services
{
    public class ValidationService : IValidationService
    {
        private readonly IXsdValidationService _xsdValidationService;
        private readonly IValidatorService _validatorService;
        private readonly ILogger<ValidationService> _logger;

        public ValidationService(
            IXsdValidationService xsdValidationService,
            IValidatorService validatorService,
            ILogger<ValidationService> logger)
        {
            _xsdValidationService = xsdValidationService;
            _validatorService = validatorService;
            _logger = logger;
        }

        public ValidationReport Validate(List<IFormFile> files, string xmlNamespace)
        {
            var startTime = DateTime.Now;

            using var inputData = GetInputData(files);
            var schemaRule = _xsdValidationService.Validate(inputData, xmlNamespace);
            var rules = new List<Rule> { schemaRule };

            var validator = _validatorService.GetValidator(xmlNamespace);
            
            rules.AddRange(validator.Validate(inputData));

            return CreateValidationReport(startTime, inputData, rules);
        }
    }
}
