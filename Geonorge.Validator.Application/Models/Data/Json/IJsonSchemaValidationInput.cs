using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Json
{
    public interface IJsonSchemaValidationInput
    {
        DisposableList<InputData> Data { get; }
        Func<InputData, List<JsonSchemaValidationError>> Validate { get; }
    }
}
