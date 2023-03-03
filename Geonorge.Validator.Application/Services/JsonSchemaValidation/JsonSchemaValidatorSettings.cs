namespace Geonorge.Validator.Application.Services.JsonSchemaValidation
{
    public class JsonSchemaValidatorSettings
    {
        public static readonly string SectionName = "JsonSchemaValidator";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] CacheableHosts { get; set; }
    }
}
