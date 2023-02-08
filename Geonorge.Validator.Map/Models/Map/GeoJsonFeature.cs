using Newtonsoft.Json.Linq;

namespace Geonorge.Validator.Map.Models.Map
{
    public class GeoJsonFeature
    {
        public string Type { get; } = "Feature";
        public JObject Geometry { get; set; }
        public JObject Properties { get; set; }
    }
}
