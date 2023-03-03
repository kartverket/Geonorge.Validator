using System.ComponentModel;

namespace Geonorge.Validator.Common.Models
{
    public enum FileType
    {
        [Description("application/xml")]
        XML,
        [Description("application/xml+gml")]
        GML32,
        [Description("application/xml")]
        XSD,
        [Description("application/json")]
        JSON,
        [Description("application/geo+json")]
        GeoJSON,
        [Description("application/octet-stream")]
        Unknown
    }
}
