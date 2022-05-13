using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.XsdValidator.Models;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public interface IXsdValidationService
    {
        XsdRule Validate(DisposableList<InputData> inputData, XsdData xsdData);
    }
}
