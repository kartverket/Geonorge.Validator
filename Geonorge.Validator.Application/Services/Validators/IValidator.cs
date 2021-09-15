using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.Validators
{
    public interface IValidator
    {
        List<Rule> Validate(DisposableList<InputData> inputData);
        List<RuleSet> GetRuleInfo(string xmlNamespace);
    }
}
