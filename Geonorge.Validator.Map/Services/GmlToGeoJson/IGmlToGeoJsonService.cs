using Geonorge.Validator.Map.Models.Map;
using System.Xml.Linq;
using AxisOrientation = Geonorge.Validator.Map.Models.Map.AxisOrientation;

namespace Geonorge.Validator.Map.Services
{
    public interface IGmlToGeoJsonService
    {
        GeoJsonFeatureCollection CreateGeoJsonDocument(XDocument document, AxisOrientation axisOrientation, Dictionary<string, string> geoElementMappings = null);
    }
}
