using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Json
{
    public class JsonSchemaValidationInput : IJsonSchemaValidationInput
    {
        private JsonSchemaValidationInput(
            DisposableList<InputData> data, Func<InputData, List<ValidationError>> validateFunc)
        {
            Data = data;
            Validate = validateFunc;
        }

        public DisposableList<InputData> Data { get; set; }
        public Func<InputData, List<ValidationError>> Validate { get; set; }

        public static IJsonSchemaValidationInput Create(
            DisposableList<InputData> data, Func<InputData, List<ValidationError>> validateFunc)
        {
            return new JsonSchemaValidationInput(data, validateFunc);
        }
    }
}
