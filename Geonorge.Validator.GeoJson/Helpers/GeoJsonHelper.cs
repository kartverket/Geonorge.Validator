using Geonorge.Validator.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Reflection;

namespace Geonorge.Validator.GeoJson.Helpers
{
    public class GeoJsonHelper
    {
        public static readonly JSchema GeoJsonSchema = LoadGeoJsonSchema();

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

        private static JSchema LoadGeoJsonSchema()
        {
            using var schemaStream = FileHelper.GetResourceStream("GeoJSON.schema.json", Assembly.Load("Geonorge.Validator.GeoJson"));
            using var jsonReader = new JsonTextReader(new StreamReader(schemaStream));
            
            return JSchema.Load(jsonReader, new JSchemaUrlResolver());
        }
    }
}
