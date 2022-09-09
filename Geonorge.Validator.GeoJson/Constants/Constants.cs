namespace Geonorge.Validator.GeoJson.Constants
{
    public class Constants
    {
        public static readonly List<Uri> GeoJsonSchemaIds = new()
        {
            new Uri("https://geojson.org/schema/GeoJSON.json"),
            new Uri("https://geojson.org/schema/FeatureCollection.json"),
            new Uri("https://geojson.org/schema/Feature.json"),
            new Uri("https://geojson.org/schema/Geometry.json"),
            new Uri("https://geojson.org/schema/GeometryCollection.json"),
            new Uri("https://geojson.org/schema/MultiPolygon.json"),
            new Uri("https://geojson.org/schema/MultiLineString.json"),
            new Uri("https://geojson.org/schema/MultiPoint.json"),
            new Uri("https://geojson.org/schema/Polygon.json"),
            new Uri("https://geojson.org/schema/LineString.json"),
            new Uri("https://geojson.org/schema/Point.json")
        };
    }
}
