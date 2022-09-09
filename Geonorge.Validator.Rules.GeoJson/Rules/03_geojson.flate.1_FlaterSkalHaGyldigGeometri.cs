using DiBK.RuleValidator;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class FlaterSkalHaGyldigGeometri : Rule<IGeoJsonValidationInput>
    {
        public override void Create()
        {
            Id = "geojson.flate.1";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
            {
                var surfaceGeometries = document.GetGeometriesByType(GeoJsonGeometry.Polygon, GeoJsonGeometry.MultiPolygon);

                foreach (var indexed in surfaceGeometries)
                {
                    if (indexed.Geometry.IsValid())
                        continue;

                    var geomToken = GeoJsonHelper.GetGeometry(indexed.Feature);
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
                }
            }
        }
    }
}
