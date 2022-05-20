using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public interface IGenericGmlValidator
    {
        Task<List<Rule>> Validate(DisposableList<InputData> inputData, Dictionary<string, Uri> codelistUris);
    }
}
