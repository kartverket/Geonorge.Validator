using DiBK.RuleValidator;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services
{
    public interface IRuleService
    {
        List<RuleSet> GetRuleInfo(string xmlNamespace);
    }
}
