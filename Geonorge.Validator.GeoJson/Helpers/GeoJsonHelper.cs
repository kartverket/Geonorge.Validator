using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using static Geonorge.Validator.GeoJson.Constants.Constants;

namespace Geonorge.Validator.GeoJson.Helpers
{
    public class GeoJsonHelper
    {
        public static IEnumerable<JToken> GetAllTokens(JToken document)
        {
            var toSearch = new Stack<JToken>(document.Children());

            while (toSearch.Count > 0)
            {
                var inspected = toSearch.Pop();
                yield return inspected;

                foreach (var child in inspected)
                    toSearch.Push(child);
            }
        }

        public static JToken GetGeometry(JToken feature) => feature["geometry"];

        public static string GetGeometryType(JToken feature) => GetGeometry(feature)?["type"]?.Value<string>();

        public static (int LineNumber, int LinePosition) GetLineInfo(JToken token)
        {
            var lineInfo = (IJsonLineInfo)token;

            return lineInfo.HasLineInfo() ?
                (lineInfo.LineNumber, lineInfo.LinePosition) :
                default;
        }

        public static string GetPropValue(JToken feature, string key)
        {
            var properties = feature["properties"];

            return (string)properties?[key];
        }

        public static T GetPropValue<T>(JToken feature, string key)
            where T : IConvertible
        {
            var properties = feature["properties"];
            var value = properties?[key];

            if (value == null)
                return default;

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception)
            {
                return default;
            }
        }

        public static bool HasGeoJson(JSchema schema)
        {
            var schemaUris = new List<Uri>();
            FindSchemaUris(schema, schemaUris);

            return schemaUris
                .Any(uri =>
                {
                    var uriWithoutScheme = GetUriWithoutScheme(uri);
                    return GeoJsonSchemaIds.Any(uri => uriWithoutScheme == GetUriWithoutScheme(uri));
                });
        }

        private static void FindSchemaUris(JSchema schema, List<Uri> schemaIds)
        {
            if (schema.Id != null)
                schemaIds.Add(schema.Id);

            if (schema.AllOf.Any())
                foreach (var sch in schema.AllOf)
                    FindSchemaUris(sch, schemaIds);

            if (schema.Properties.Any())
                foreach (var (_, sch) in schema.Properties)
                    FindSchemaUris(sch, schemaIds);
        }

        private static string GetUriWithoutScheme(Uri uri) => uri.Host + uri.PathAndQuery + uri.Fragment;
    }
}
