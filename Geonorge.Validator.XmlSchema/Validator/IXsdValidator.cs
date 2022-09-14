using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using System.Threading.Tasks;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public interface IXsdValidator
    {
        Task<XsdValidatorResult> ValidateAsync(InputData inputData, XmlSchemaData xsdData);
    }
}
