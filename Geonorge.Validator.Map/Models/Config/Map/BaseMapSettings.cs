namespace Geonorge.Validator.Map.Models.Config.Map
{
    public class BaseMapSettings
    {
        public string Name { get; set; }
        public string WmsUrl { get; set; }
        public string WmtsCapabilitiesUrl { get; set; }
        public string Layer { get; set; }
        public int MaxZoom { get; set; }
        public int Equidistance { get; set; }
    }
}
