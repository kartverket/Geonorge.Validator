using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.XmlSchema.Models;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public interface IXmlSchemaValidationService
    {
        Task<XsdValidationResult> ValidateAsync(DisposableList<InputData> inputData, XmlSchemaData xsdData);
    }
}
