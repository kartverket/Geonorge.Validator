using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson.Constants;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using static Geonorge.Validator.GeoJson.Helpers.GeometryHelper;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class HullKanIkkeOverlappeAndreHullISammeFlate : Rule<IGeoJsonValidationInput>
    {
        private readonly ConcurrentBag<JToken> _invalidTokens = new();

        public override void Create()
        {
            Id = "geojson.flate.5";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            SetData(DataKey.OverlappingHoles + document.Id, _invalidTokens);

            var surfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon);

            foreach (var indexed in surfaceGeometries)
            {
                var geometry = indexed.Geometry;
                var feature = indexed.Feature;
                var ringsList = GetRingsOfSurface(geometry);

                foreach (var (_, InteriorRings) in ringsList)
                {
                    if (InteriorRings.Count < 2)
                        continue;

                    for (int i = 0; i < InteriorRings.Count - 1; i++)
                    {
                        var interiorRing = InteriorRings[i];

                        Parallel.For(i + 1, InteriorRings.Count, index =>
                        {
                            var otherInteriorRing = InteriorRings[index];

                            if (interiorRing.Overlaps(otherInteriorRing))
                            {
                                var geomToken = GeoJsonHelper.GetGeometry(feature);
                                var geomType = geomToken["type"];
                                var coordToken = geomToken["coordinates"];
                                var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);
                                using var intersection = interiorRing.Intersection(otherInteriorRing);
                                intersection.ExportToWkt(out var intersectionWkt);

                                this.AddMessage(
                                    Translate("Message", geomType),
                                    document.FileName,
                                    coordToken.Path,
                                    LineNumber,
                                    LinePosition,
                                    intersectionWkt
                                );

                                _invalidTokens.Add(feature);
                            }
                        });
                    }
                }
            }
        }
    }
}

