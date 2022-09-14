using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Json
{
    public class JsonSchemaValidationInput : IJsonSchemaValidationInput
    {
        private JsonSchemaValidationInput(
            DisposableList<InputData> data, Func<InputData, List<JsonSchemaValidationError>> validateFunc)
        {
            Data = data;
            Validate = validateFunc;
        }

        public DisposableList<InputData> Data { get; set; }
        public Func<InputData, List<JsonSchemaValidationError>> Validate { get; set; }

        public static IJsonSchemaValidationInput Create(
            DisposableList<InputData> data, Func<InputData, List<JsonSchemaValidationError>> validateFunc)
        {
            return new JsonSchemaValidationInput(data, validateFunc);
        }
    }
}
