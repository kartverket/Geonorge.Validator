using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson.Constants;
using Newtonsoft.Json.Linq;
using OSGeo.OGR;
using System.Collections.Concurrent;
using System.Globalization;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class AvgrensningenTilEnFlateKanIkkeKrysseSegSelv : Rule<IGeoJsonValidationInput>
    {
        private readonly ConcurrentBag<JToken> _invalidTokens = new();

        public override void Create()
        {
            Id = "geojson.flate.2";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            SetData(DataKey.SelfIntersections + document.Id, _invalidTokens);

            var indexedSurfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon)
                .Where(indexed => !indexed.IsValid)
                .ToList();

            foreach (var indexed in indexedSurfaceGeometries)
            {
                if (indexed.Geometry == null || indexed.Geometry.IsSimple())
                    continue;

                DetectSelfIntersection(document, indexed.Feature, indexed.Geometry);
            }
        }

        private void DetectSelfIntersection(GeoJsonDocument document, JToken feature, Geometry surface)
        {
            using var point = GeometryHelper.DetectSelfIntersection(surface);

            if (point == null)
                return;

            var pointX = point.GetX(0).ToString(CultureInfo.InvariantCulture);
            var pointY = point.GetY(0).ToString(CultureInfo.InvariantCulture);

            var geomToken = GeoJsonHelper.GetGeometry(feature);
            var geomType = geomToken["type"];
            var coordToken = geomToken["coordinates"];
            var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);

            this.AddMessage(
                Translate("Message", geomType, pointX, pointY),
                document.FileName,
                coordToken.Path,
                LineNumber,
                LinePosition
            );

            _invalidTokens.Add(feature);
        }
    }
}
