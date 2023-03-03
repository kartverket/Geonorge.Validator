using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using Newtonsoft.Json.Schema;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public interface IJsonSchemaValidationService
    {
        Task<JsonSchemaValidationResult> ValidateAsync(DisposableList<InputData> inputData, JSchema schema);
    }
}
