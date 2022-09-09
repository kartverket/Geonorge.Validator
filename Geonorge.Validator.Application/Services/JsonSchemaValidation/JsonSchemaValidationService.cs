using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public class JsonSchemaValidationService : IJsonSchemaValidationService
    {
        public List<ValidationError> Validate(InputData inputData, JSchema schema)
        {
            using var jsonReader = new JsonTextReader(new StreamReader(inputData.Stream, leaveOpen: true));
            var document = JToken.Load(jsonReader);

            var validationErrors = new List<ValidationError>();
            void OnValidationError(object sender, SchemaValidationEventArgs args) => validationErrors.Add(args.ValidationError);

            document.Validate(schema, new SchemaValidationEventHandler(OnValidationError));           

            inputData.IsValid = !validationErrors.Any();
            inputData.Stream.Position = 0;

            return validationErrors;
        }
    }
}
