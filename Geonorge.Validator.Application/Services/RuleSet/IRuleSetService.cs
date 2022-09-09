using Geonorge.Validator.Application.Models.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.RuleSetService
{
    public interface IRuleSetService
    {
        List<RuleSet> GetRuleSets();
        Task<List<RuleSet>> GetRuleSetsForDataset();
    }
}
