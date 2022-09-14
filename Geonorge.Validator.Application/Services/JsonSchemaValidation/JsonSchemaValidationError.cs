namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public class JsonSchemaValidationError
    {
        public string Message { get; set; }
        public string JsonPath { get; set; }
        public int LineNumber { get; set; }
        public int  LinePosition { get; set; }
    }
}
