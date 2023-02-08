namespace Geonorge.Validator.Map.Models.Config.Styling
{
    public class MapStyling
    {
        public string Namespace { get; set; }
        public List<MapStylingLayer> Layers { get; set; } = new();
    }
}
