using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Services.Validators.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.Validators
{
    public class PlanomrissValidator : IPlanomrissValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ValidatorOptions _options;

        public PlanomrissValidator(
            IRuleValidator validator,
            IOptions<ValidatorOptions> options)
        {
            _validator = validator;
            _options = options.Value;
        }

        public List<Rule> Validate(DisposableList<InputData> inputData)
        {
            var validationData = GmlValidationData.Create(inputData);

            _validator.Validate<IGmlValidationData>(validationData, options =>
            {
                options.SkipRule<KoordinatreferansesystemForKart3D>();
                options.SkipRule<KurverSkalHaGyldigGeometri>();
                options.SkipRule<BueKanIkkeHaDobbeltpunkter>();
                options.SkipRule<BueKanIkkeHaPunkterPåRettLinje>();
            });

            return _validator.GetAllRules();
        }

        public List<RuleSet> GetRuleInfo(string xmlNamespace)
        {
            var ruleTypes = _options.GetRuleTypes(xmlNamespace);

            return _validator.GetRuleInfo(ruleTypes, options =>
            {
                options.SkipRule<KoordinatreferansesystemForKart3D>();
                options.SkipRule<KurverSkalHaGyldigGeometri>();
                options.SkipRule<BueKanIkkeHaDobbeltpunkter>();
                options.SkipRule<BueKanIkkeHaPunkterPåRettLinje>();
            });
        }
    }
}
