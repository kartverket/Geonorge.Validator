namespace Geonorge.Validator.Map.Models.Config.Styling
{
    public class MapStylingLayer
    {
        public string Name { get; set; }
        public string SLD { get; set; }
        public int ZIndex { get; set; }
        public bool ShowLegend { get; set; } = true;
    }
}
