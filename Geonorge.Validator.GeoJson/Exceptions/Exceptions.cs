using System;

namespace Geonorge.Validator.GeoJson.Exceptions
{
    public class GeometryFromGeoJsonException : Exception
    {
        public GeometryFromGeoJsonException()
        {
        }

        public GeometryFromGeoJsonException(string message) : base(message)
        {
        }

        public GeometryFromGeoJsonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
