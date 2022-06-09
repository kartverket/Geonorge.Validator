using Geonorge.Validator.Application.Models.Data;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.RuleInfoService
{
    public interface IRuleInfoService
    {
        List<RuleSet> GetRuleInfo();
    }
}
