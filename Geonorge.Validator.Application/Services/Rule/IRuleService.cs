using DiBK.RuleValidator;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.RuleService
{
    public interface IRuleService
    {
        List<RuleSetGroup> GetRuleInfo(string xmlNamespace);
    }
}
