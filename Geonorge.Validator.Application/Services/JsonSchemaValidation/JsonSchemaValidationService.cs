using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Services.JsonSchemaValidation.Translator;
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
        public List<JsonSchemaValidationError> Validate(InputData inputData, JSchema schema)
        {
            using var jsonReader = new JsonTextReader(new StreamReader(inputData.Stream, leaveOpen: true));
            var document = JToken.Load(jsonReader);

            var validationErrors = new List<JsonSchemaValidationError>();

            void OnValidationError(object sender, SchemaValidationEventArgs args)
            {
                var lineNumber = args.ValidationError.LineNumber;
                var linePosition = args.ValidationError.LinePosition;
                var message = $"Linje {lineNumber}, posisjon {linePosition}: {MessageTranslator.TranslateError(args.ValidationError.Message)}";

                var validationError = new JsonSchemaValidationError
                {
                    Message = message,
                    JsonPath = args.ValidationError.Path,
                    LineNumber = lineNumber,
                    LinePosition = linePosition
                };

                validationErrors.Add(validationError);
            };

            document.Validate(schema, new SchemaValidationEventHandler(OnValidationError));           

            inputData.IsValid = !validationErrors.Any();
            inputData.Stream.Position = 0;

            return validationErrors;
        }
    }
}
