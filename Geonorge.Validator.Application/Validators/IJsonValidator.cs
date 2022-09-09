using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Validators
{
    public interface IJsonValidator : IValidator
    {
        Task<List<Rule>> ValidateAsync(string schemaId, DisposableList<InputData> inputData, List<string> skipRules);
    }
}
