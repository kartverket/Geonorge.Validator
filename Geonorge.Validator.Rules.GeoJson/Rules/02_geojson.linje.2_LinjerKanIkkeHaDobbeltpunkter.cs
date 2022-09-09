using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class LinjerKanIkkeHaDobbeltpunkter : Rule<IGeoJsonValidationInput>
    {
        public override void Create()
        {
            Id = "geojson.linje.2";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            var lineGeometries = document.GetGeometriesByType(GeoJsonGeometry.LineString, GeoJsonGeometry.MultiLineString);

            Parallel.ForEach(lineGeometries, indexed =>
            {
                var pointTuples = new List<(double[] PointA, double[] PointB)>();
                var feature = indexed.Feature;
                var geometry = indexed.Geometry;

                if (geometry.GetGeometryType() == wkbGeometryType.wkbLineString)
                {
                    FindDoublePoints(document, feature, geometry);
                }
                else if (geometry.GetGeometryType() == wkbGeometryType.wkbMultiLineString)
                {
                    var geomCount = geometry.GetGeometryCount();

                    for (var i = 0; i < geomCount; i++)
                    {
                        using var lineString = geometry.GetGeometryRef(i);
                        FindDoublePoints(document, feature, lineString);
                    }
                }
            });
        }

        private void FindDoublePoints(GeoJsonDocument document, JToken feature, Geometry lineString)
        {
            var pointTuples = new List<(double[] PointA, double[] PointB)>();
            var points = lineString.GetPoints();

            for (var i = 1; i < points.Length; i++)
                pointTuples.Add((points[i - 1], points[i]));

            var doublePoint = pointTuples
                .FirstOrDefault(tuple => tuple.PointA[0] == tuple.PointB[0] && 
                    tuple.PointA[1] == tuple.PointB[1]);

            if (doublePoint != default)
            {
                var x = doublePoint.PointA[0];
                var y = doublePoint.PointA[1];

                using var point = GeometryHelper.CreatePoint(x, y);
                FormattableString pointString = $"{x}, {y}";

                var geomToken = GeoJsonHelper.GetGeometry(feature);
                var geomType = geomToken["type"];
                var coordToken = geomToken["coordinates"];
                var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);

                this.AddMessage(
                    Translate("Message", geomType, pointString),
                    document.FileName,
                    coordToken.Path,
                    LineNumber,
                    LinePosition
                );
            }
        }
    }
}
