using Geonorge.Validator.GeoJson.Models;

namespace Geonorge.Validator.Rules.GeoJson
{
    public interface IGeoJsonValidationInput : IDisposable
    {
        List<GeoJsonDocument> Documents { get; }
    }
}
