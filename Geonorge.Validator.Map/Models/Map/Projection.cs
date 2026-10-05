using OSGeo.OSR;
using AO = OSGeo.OSR.AxisOrientation;

namespace Geonorge.Validator.Map.Models.Map
{
    public class Projection
    {
        // Nyere EPSG-database (GDAL 3.13) bruker ETRS89-NOR [EUREF89] / UTM (11021-11026) som horisontal del av
        // sammensatte koordinatsystemer (f.eks. 5972). Disse mappes til ETRS89 / UTM (25831-25836) som resten av løsningen bruker.
        private static readonly Dictionary<int, int> _etrs89NorUtmCodes = new()
        {
            { 11021, 25831 },
            { 11022, 25832 },
            { 11023, 25833 },
            { 11024, 25834 },
            { 11025, 25835 },
            { 11026, 25836 }
        };

        public Epsg Epsg { get; set; }
        public Epsg Epsg2d { get; set; }
        public string Description { get; set; }
        public AxisOrientation AxisOrientation { get; set; }
        public string Uri { get; set; }
        public string Urn { get; set; }
        public string Proj4 { get; set; }

        public static Projection Create(int code)
        {
            try
            {
                using var spatialReference = new SpatialReference(null);
                spatialReference.ImportFromEPSG(code);

                var projection = new Projection
                {
                    Epsg = new Epsg(code),
                    Epsg2d = new Epsg(code),
                    Description = spatialReference.GetName(),
                    AxisOrientation = GetAxisOrientation(spatialReference),
                    Uri = $"http://www.opengis.net/def/crs/EPSG/0/{code}",
                    Urn = $"urn:ogc:def:crs:EPSG::{code}",
                };

                if (spatialReference.IsCompound() == 1)
                {
                    var authCode = spatialReference.GetAuthorityCode("projcs");

                    if (int.TryParse(authCode, out var code2d))
                        projection.Epsg2d = new Epsg(_etrs89NorUtmCodes.GetValueOrDefault(code2d, code2d));
                }

                spatialReference.ExportToProj4(out string proj4);

                if (projection.AxisOrientation == AxisOrientation.neu)
                    proj4 += " +axis=neu";

                projection.Proj4 = proj4;

                return projection;
            }
            catch
            {
                return null;
            }
        }

        private static AxisOrientation GetAxisOrientation(SpatialReference spatialReference)
        {
            AO ao1 = AO.OAO_East;
            AO ao2 = AO.OAO_North;

            if (spatialReference.IsProjected() == 1)
            {
                ao1 = spatialReference.GetAxisOrientation("projcs", 0);
                ao2 = spatialReference.GetAxisOrientation("projcs", 1);
            }
            else if (spatialReference.IsGeographic() == 1)
            {
                ao1 = spatialReference.GetAxisOrientation("geogcs", 0);
                ao2 = spatialReference.GetAxisOrientation("geogcs", 1);
            }

            if (ao1 == AO.OAO_East && ao2 == AO.OAO_North)
                return AxisOrientation.enu;
            else if (ao1 == AO.OAO_North && ao2 == AO.OAO_East)
                return AxisOrientation.neu;

            return AxisOrientation.enu;
        }
    }
}
