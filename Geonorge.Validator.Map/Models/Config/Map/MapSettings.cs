using Geonorge.Validator.Map.Models.Map;

namespace Geonorge.Validator.Map.Models.Config.Map
{
    public class MapSettings
    {
        public static string SectionName => "Map";
        public BaseMapSettings BaseMap { get; set; }
        public List<WmsExtentSetting> WmtsExtents { get; set; }
        public List<int> SupportedEpsgCodes { get; set; }
        public List<int> BaseMapEpsgCodes { get; set; }
        public List<Projection> Projections { get; set; }
    }
}

