using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Rules.JsonSchema;
using Geonorge.Validator.Application.Services.JsonSchemaValidation.Translator;
using Geonorge.Validator.Common.Helpers;
using Geonorge.Validator.GeoJson.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public class JsonSchemaValidationService : IJsonSchemaValidationService
    {
        private readonly ILogger<JsonSchemaValidationService> _logger;

        public JsonSchemaValidationService(
            ILogger<JsonSchemaValidationService> logger)
        {
            _logger = logger;
        }

        public async Task<JsonSchemaValidationResult> ValidateAsync(DisposableList<InputData> inputData, JSchema schema)
        {
            var jsonSchemaRule = GetJsonSchemaRule();
            var startTime = DateTime.Now;
            var geoJsonFiles = new List<string>();

            foreach (var data in inputData)
            {
                var document = await JsonHelper.LoadJsonDocumentAsync(data.Stream);
                var validationErrors = Validate(document, schema);

                validationErrors
                    .Select(error =>
                    {
                        var properties = new Dictionary<string, object>
                        {
                            { "LineNumber", error.LineNumber },
                            { "LinePosition", error.LinePosition },
                            { "JsonPath", error.JsonPath },
                            { "FileName", data.FileName }
                        };

                        return new RuleMessage
                        {
                            Message = error.Message,
                            Properties = properties
                        };
                    })
                    .ToList()
                    .ForEach(jsonSchemaRule.AddMessage);

                data.IsValid = !validationErrors.Any();

                if (IsGeoJson(document))
                    geoJsonFiles.Add(data.FileName);
            }

            jsonSchemaRule.Status = !jsonSchemaRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(jsonSchemaRule, startTime);

            return new JsonSchemaValidationResult(jsonSchemaRule, geoJsonFiles);
        }

        private List<JsonSchemaValidationError> Validate(JToken document, JSchema schema)
        {
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

            return validationErrors;
        }

        private void LogInformation(JsonSchemaRule jsonSchemaRule, DateTime startTime)
        {
            _logger.LogInformation("{@Rule}", new
            {
                jsonSchemaRule.Id,
                jsonSchemaRule.Name,
                FullName = jsonSchemaRule.ToString(),
                jsonSchemaRule.Status,
                TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds,
                MessageCount = jsonSchemaRule.Messages.Count
            });
        }

        private static JsonSchemaRule GetJsonSchemaRule()
        {
            var rule = Activator.CreateInstance(typeof(SkjemavalideringJson)) as JsonSchemaRule;
            rule.Create();

            return rule;
        }

        private static bool IsGeoJson(JToken document)
        {
            return document.IsValid(GeoJsonHelper.GeoJsonSchema);
        }
    }
}
