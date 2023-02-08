using System.Collections.Concurrent;

namespace Geonorge.Validator.Map.Models.Map
{
    public class GeoJsonFeatureCollection
    {
        public string Type { get; } = "FeatureCollection";
        public ConcurrentBag<GeoJsonFeature> Features { get; set; } = new();
    }
}
