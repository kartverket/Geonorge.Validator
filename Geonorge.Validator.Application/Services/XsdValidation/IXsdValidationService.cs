using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using System.IO;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public interface IXsdValidationService
    {
        XsdRule Validate(DisposableList<InputData> inputData, Stream xsdStream);
    }
}
