using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using static DiBK.RuleValidator.Extensions.Gml.GeometryHelper;
using static Geonorge.Validator.GeoJson.Helpers.GeometryHelper;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class AvgrensningeneTilEnFlateSkalNøstesRiktig : Rule<IGeoJsonValidationInput>
    {
        public override void Create()
        {
            Id = "geojson.flate.3";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            var surfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon);

            foreach (var indexed in surfaceGeometries)
            {
                var geometry = indexed.Geometry;
                var feature = indexed.Feature;
                var ringsList = GetRingsOfSurface(geometry);

                foreach (var (ExteriorRing, InteriorRings) in ringsList)
                {
                    var exteriorPoints = ExteriorRing.GetPoints().ToList();

                    if (PointsAreClockWise(exteriorPoints))
                    {
                        var geomToken = GeoJsonHelper.GetGeometry(feature);
                        var geomType = geomToken["type"];
                        var coordToken = geomToken["coordinates"];
                        var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);

                        this.AddMessage(
                            Translate("Message1", geomType),
                            document.FileName,
                            coordToken.Path,
                            LineNumber,
                            LinePosition
                        );
                    }

                    foreach (var interiorRing in InteriorRings)
                    {
                        var interiorPoints = interiorRing.GetPoints().ToList();

                        if (!PointsAreClockWise(interiorPoints))
                        {
                            using var polygon = CreatePolygonFromRing(interiorRing);
                            using var point = polygon.PointOnSurface();
                            var geomToken = GeoJsonHelper.GetGeometry(feature);
                            var geomType = geomToken["type"];
                            var coordToken = geomToken["coordinates"];
                            var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);

                            this.AddMessage(
                                Translate("Message2", geomType),
                                document.FileName,
                                coordToken.Path,
                                LineNumber,
                                LinePosition,
                                GetZoomToPoint(point)
                            );
                        }
                    }
                }

                ringsList.ForEach(rings =>
                {
                    rings.ExteriorRing.Dispose();
                    rings.InteriorRings.ForEach(ring => ring.Dispose());
                });
            }
        }
    }
}
