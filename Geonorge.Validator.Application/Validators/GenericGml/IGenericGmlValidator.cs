using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public interface IGenericGmlValidator
    {
        Task<List<Rule>> Validate(
            DisposableList<InputData> inputData, HashSet<XmlSchemaElement> xmlSchemaElements, XmlSchemaSet xmlSchemaSet, List<string> skipRules);
    }
}
