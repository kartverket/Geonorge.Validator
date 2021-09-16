﻿using DiBK.RuleValidator;
using Geonorge.Validator.Application.Services.Validators.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services
{
    public class RuleService : IRuleService
    {
        private readonly IRuleValidator _validator;
        private readonly ValidatorOptions _options;

        public RuleService(
            IRuleValidator validator,
            IOptions<ValidatorOptions> options)
        {
            _validator = validator;
            _options = options.Value;
        }

        public List<RuleSet> GetRuleInfo(string xmlNamespace)
        {
            var ruleTypes = _options.GetRuleTypes(xmlNamespace);
            var validationOptions = _options.GetValidationOptions(xmlNamespace);

            return _validator.GetRuleInfo(ruleTypes, validationOptions);
        }
    }
}
