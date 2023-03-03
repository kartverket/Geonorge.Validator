using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class JsonSchemaValidationResult
    {
        public JsonSchemaValidationResult(
            JsonSchemaRule rule, List<string> geoJsonFiles)
        {
            Rule = rule;
            GeoJsonFiles = geoJsonFiles;
        }

        public JsonSchemaRule Rule { get; private set; }
        public List<string> GeoJsonFiles { get; set; }
    }
}
