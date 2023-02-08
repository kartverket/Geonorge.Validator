using Geonorge.Validator.Map.Models.Config.Styling;

namespace Geonorge.Validator.Map.Models.Map
{
    public class MapDocument
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public Projection Projection { get; set; }
        public GeoJsonFeatureCollection GeoJson { get; set; } = new();
        public MapStyling Styling { get; set; }
    }
}
