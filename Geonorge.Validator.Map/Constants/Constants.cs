using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Xml.Linq;

namespace Geonorge.Validator.Map.Constants
{
    public class Constants
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = CreateDefaultJsonSerializerSettings();

        public static readonly XNamespace GmlNs = "http://www.opengis.net/gml/3.2";

        public static readonly string[] GmlGeometryElementNames = new[]
        {
            "CompositeCurve",
            "CompositeSolid",
            "CompositeSurface",
            "Curve",
            "GeometricComplex",
            "Grid",
            "LineString",
            "MultiCurve",
            "MultiGeometry",
            "MultiPoint",
            "MultiSolid",
            "MultiSurface",
            "OrientableCurve",
            "OrientableSurface",
            "Point",
            "Polygon",
            "PolyhedralSurface",
            "RectifiedGrid",
            "Solid",
            "Surface",
            "Tin",
            "TriangulatedSurface"
        };

        private static JsonSerializerSettings CreateDefaultJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
            };

            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}
