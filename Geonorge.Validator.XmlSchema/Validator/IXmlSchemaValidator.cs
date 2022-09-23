using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using System.Threading.Tasks;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public interface IXmlSchemaValidator
    {
        Task<XmlSchemaValidatorResult> ValidateAsync(InputData inputData, XmlSchemaData xmlSchemaData, string xmlNamespace);
    }
}
