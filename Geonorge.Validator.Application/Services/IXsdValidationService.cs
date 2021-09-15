using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Services
{
    public interface IXsdValidationService
    {
        SchemaRule Validate(DisposableList<InputData> inputData, string xmlNamespace);
    }
}
