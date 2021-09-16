using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.Validators
{
    public class ReguleringsplanforslagValidator : IReguleringsplanforslagValidator
    {
        private readonly IRuleValidator _validator;

        public ReguleringsplanforslagValidator(
            IRuleValidator validator)
        {
            _validator = validator;
        }

        public List<Rule> Validate(string xmlNamespace, DisposableList<InputData> inputData)
        {
            using var validationData = GmlValidationData.Create(inputData);

            _validator.Validate(validationData);

            return _validator.GetAllRules();
        }
    }
}
