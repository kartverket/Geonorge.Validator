using DiBK.RuleValidator;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services
{
    public class RuleService : IRuleService
    {
        private readonly IValidatorService _validatorService;

        public RuleService(
            IValidatorService validatorService)
        {
            _validatorService = validatorService;
        }

        public List<RuleSet> GetRuleInfo(string xmlNamespace)
        {
            var validator = _validatorService.GetValidator(xmlNamespace);

            return validator.GetRuleInfo(xmlNamespace);
        }
    }
}
