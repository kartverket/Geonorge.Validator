using Geonorge.Validator.GeoJson.Exceptions;
using OSGeo.OGR;

namespace Geonorge.Validator.GeoJson.Helpers
{
    public class GeometryHelper
    {
        public static (Geometry ExteriorRing, List<Geometry> InteriorRings) GetRingsOfPolygon(Geometry polygon)
        {
            if (polygon.GetGeometryType() != wkbGeometryType.wkbPolygon && polygon.GetGeometryType() != wkbGeometryType.wkbPolygon25D)
                throw new GeometryException($"Geometrien ({polygon.GetGeometryType()}) er ikke et polygon.");

            var geomCount = polygon.GetGeometryCount();
            var interior = new List<Geometry>();

            if (geomCount > 1)
            {
                for (var i = 1; i < geomCount; i++)
                    interior.Add(polygon.GetGeometryRef(i));
            }

            return (polygon.GetGeometryRef(0), interior);
        }

        public static List<(Geometry ExteriorRing, List<Geometry> InteriorRings)> GetRingsOfSurface(Geometry geometry)
        {
            var ringsList = new List<(Geometry ExteriorRing, List<Geometry> InteriorRings)>();

            if (geometry.GetGeometryType() == wkbGeometryType.wkbPolygon || geometry.GetGeometryType() == wkbGeometryType.wkbPolygon25D)
            {
                ringsList.Add(GetRingsOfPolygon(geometry));
            }
            else if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiPolygon || geometry.GetGeometryType() == wkbGeometryType.wkbMultiPolygon25D)
            {
                var geomCount = geometry.GetGeometryCount();

                for (var i = 1; i < geomCount; i++)
                {
                    var polygon = geometry.GetGeometryRef(i);
                    ringsList.Add(GetRingsOfPolygon(polygon));
                }
            }
            else
            {
                throw new GeometryException($"Geometrien ({geometry.GetGeometryType()}) er ikke et polygon eller multipolygon.");
            }

            return ringsList;
        }
    }
}
