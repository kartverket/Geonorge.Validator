using DiBK.RuleValidator;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson.Constants;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class FlaterSkalHaGyldigGeometri : Rule<IGeoJsonValidationInput>
    {
        public override void Create()
        {
            Id = "geojson.flate.1";

            DependOn<AvgrensningenTilEnFlateKanIkkeKrysseSegSelv>().ToExecute();
            DependOn<HullMåLiggeInnenforFlatensYtreAvgrensning>().ToExecute();
            DependOn<HullKanIkkeOverlappeAndreHullISammeFlate>().ToExecute();
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
                Validate(document);
        }

        private void Validate(GeoJsonDocument document)
        {
            var indexedSurfaceGeometries = GetInvalidIndexedSurfaceGeometries(document);

            foreach (var indexed in indexedSurfaceGeometries)
            {
                var geomToken = GeoJsonHelper.GetGeometry(indexed.Feature);
                var geomType = geomToken["type"];
                var coordToken = geomToken["coordinates"];
                var (LineNumber, LinePosition) = GeoJsonHelper.GetLineInfo(coordToken);
                
                var errorMessage = indexed.Geometry == null ?
                    indexed.ErrorMessage ?? "Flaten har ugyldig geometri." :
                    Translate("Message", geomType);

                this.AddMessage(
                    errorMessage,
                    document.FileName,
                    coordToken.Path,
                    LineNumber,
                    LinePosition
                );
            }
        }

        private List<IndexedGeoJsonGeometry> GetInvalidIndexedSurfaceGeometries(GeoJsonDocument document)
        {
            var indexedSurfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon)
                .Where(indexed => !indexed.IsValid)
                .ToList();

            var invalidSurfaceElements = GetInvalidSurfaceElements(document.Id);

            if (!invalidSurfaceElements.Any())
                return indexedSurfaceGeometries.ToList();

            return indexedSurfaceGeometries
                .ToList()
                .Where(indexed => !invalidSurfaceElements.Contains(indexed.Feature))
                .ToList();
        }

        private HashSet<JToken> GetInvalidSurfaceElements(string documentId)
        {
            var selfIntersections = GetData<ConcurrentBag<JToken>>(DataKey.SelfIntersections + documentId);
            var overlappingHoles = GetData<ConcurrentBag<JToken>>(DataKey.OverlappingHoles + documentId);
            var holesOutsideBoundary = GetData<ConcurrentBag<JToken>>(DataKey.HolesOutsideBoundary + documentId);

            var elements = new HashSet<JToken>();
            elements.UnionWith(selfIntersections);
            elements.UnionWith(overlappingHoles);
            elements.UnionWith(holesOutsideBoundary);

            return elements;
        }
    }
}
