using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.XsdValidator.Models;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public interface IXmlSchemaValidationService
    {
        XsdValidationResult Validate(DisposableList<InputData> inputData, XsdData xsdData);
    }
}
