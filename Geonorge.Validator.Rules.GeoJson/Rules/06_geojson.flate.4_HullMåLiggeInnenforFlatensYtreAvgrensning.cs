using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson.Constants;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System.Collections.Concurrent;
using static DiBK.RuleValidator.Extensions.Gml.GeometryHelper;
using static Geonorge.Validator.GeoJson.Helpers.GeometryHelper;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class HullMåLiggeInnenforFlatensYtreAvgrensning : Rule<IGeoJsonValidationInput>
    {
        private readonly ConcurrentBag<JToken> _invalidTokens = new();

        public override void Create()
        {
            Id = "geojson.flate.4";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            SetData(DataKey.HolesOutsideBoundary + document.Id, _invalidTokens);

            var surfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon);

            foreach (var indexed in surfaceGeometries)
            {
                var geometry = indexed.Geometry;
                var feature = indexed.Feature;
                var ringsList = GetRingsOfSurface(geometry);

                foreach (var (ExteriorRing, InteriorRings) in ringsList)
                {
                    Geometry exteriorPolygon;

                    try
                    {
                        exteriorPolygon = CreatePolygonFromRing(ExteriorRing);
                    }
                    catch
                    {
                        continue;
                    }

                    Parallel.ForEach(InteriorRings, interiorRing =>
                    {
                        try
                        {
                            using var interiorPolygon = CreatePolygonFromRing(interiorRing);

                            if (!exteriorPolygon.Contains(interiorPolygon))
                            {
                                var geomToken = GeoJsonHelper.GetGeometry(feature);
                                var geomType = geomToken["type"];
                                var coordToken = geomToken["coordinates"];
                                var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);

                                this.AddMessage(
                                    Translate("Message", geomType),
                                    document.FileName,
                                    coordToken.Path,
                                    LineNumber,
                                    LinePosition
                                );

                                _invalidTokens.Add(feature);
                            }
                        }
                        catch
                        {
                        }
                    });

                    exteriorPolygon.Dispose();
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
