/*using Newtonsoft.Json.Linq;
using NJsonSchema.Validation;
using System.Linq;

namespace Geonorge.Validator.Application.Models.Data.Json
{
    public class JsonValidationError
    {
        private JsonValidationError(
            ValidationErrorKind kind, string message, string errorValue, string possibleValues, string jsonPath, int lineNumber, int linePosition, string fileName)
        {
            Kind = kind;
            Message = message;
            ErrorValue = errorValue;
            PossibleValues = possibleValues;
            JsonPath = jsonPath.TrimStart('#', '/');
            LineNumber = lineNumber;
            LinePosition = linePosition;
            FileName = fileName;
        }

        public ValidationErrorKind Kind { get; set; }
        public string Message { get; set; }
        public string ErrorValue { get; set; }
        public string PossibleValues { get; set; }
        public string JsonPath { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string FileName { get; set; }

        public static JsonValidationError Create(ValidationError error, JToken document, string fileName)
        {
            var jsonPath = error.Path.TrimStart('#', '/');
            var errorValue = document.SelectToken(jsonPath)?.Value<object>().ToString();
            string possibleValues = null;

            if (error.Kind == ValidationErrorKind.NotInEnumeration)
                possibleValues = string.Join(", ", error.Schema.Enumeration.Select(enumValue => enumValue.ToString()));

            return new JsonValidationError(
                error.Kind,
                error.ToString(),
                errorValue,
                possibleValues,
                jsonPath,
                error.LineNumber,
                error.LinePosition,
                fileName
            );
        }
    }
}
*/