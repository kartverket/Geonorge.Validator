using Geonorge.Validator.GeoJson.Exceptions;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;

namespace Geonorge.Validator.GeoJson.Models
{
    public class IndexedGeoJsonGeometry : IDisposable
    {
        private bool _disposed = false;

        private IndexedGeoJsonGeometry(JToken feature, Geometry geometry, string type, string errorMessage)
        {
            Feature = feature;
            Geometry = geometry;
            Type = type;
            ErrorMessage = errorMessage;
        }

        public JToken Feature { get; private set; }
        public Geometry Geometry { get; private set; }
        public string Type { get; private set; }
        public string ErrorMessage { get; private set; }

        public bool IsValid
        {
            get
            {
                try
                {
                    return Geometry != null && Geometry.IsValid();
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && Geometry != null)
                    Geometry.Dispose();

                _disposed = true;
            }
        }

        public static IndexedGeoJsonGeometry Create(JToken feature)
        {
            Geometry geometry = null;
            string errorMessage = null;
            var geomToken = feature["geometry"];
            var type = geomToken?["type"]?.Value<string>();

            if (geomToken == null || type == null)
                return null;

            try
            {
                geometry = Ogr.CreateGeometryFromJson(geomToken.ToString());
            }
            catch (GeometryFromGeoJsonException exception)
            {
                errorMessage = exception.Message;
            }

            return new IndexedGeoJsonGeometry(feature, geometry, type, errorMessage);
        }
    }
}
