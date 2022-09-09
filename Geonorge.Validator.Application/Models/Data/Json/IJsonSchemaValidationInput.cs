using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Json
{
    public interface IJsonSchemaValidationInput
    {
        DisposableList<InputData> Data { get; }
        Func<InputData, List<ValidationError>> Validate { get; }
    }
}
