namespace Geonorge.Validator.Map.Models.Config.Styling
{
    public class StylingSettings
    {
        public static string SectionName => "Styling";
        public Dictionary<string, MapStyling> Specifications { get; set; }
    }
}
