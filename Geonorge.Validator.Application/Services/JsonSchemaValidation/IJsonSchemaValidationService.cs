using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public interface IJsonSchemaValidationService
    {
        List<JsonSchemaValidationError> Validate(InputData inputData, JSchema schema);
    }
}
