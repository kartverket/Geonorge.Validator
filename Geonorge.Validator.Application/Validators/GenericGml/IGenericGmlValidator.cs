using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public interface IGenericGmlValidator
    {
        Task<List<Rule>> Validate(DisposableList<InputData> inputData, Stream xsdStream, string gmlVersion);
    }
}
