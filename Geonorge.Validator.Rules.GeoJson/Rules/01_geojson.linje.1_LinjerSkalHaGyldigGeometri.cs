using DiBK.RuleValidator;
using Geonorge.Validator.GeoJson.Constants;
using Geonorge.Validator.GeoJson.Extensions;
using Geonorge.Validator.GeoJson.Helpers;

namespace Geonorge.Validator.Rules.GeoJson
{
    public class LinjerSkalHaGyldigGeometri : Rule<IGeoJsonValidationInput>
    {
        public override void Create()
        {
            Id = "geojson.linje.1";
        }

        protected override void Validate(IGeoJsonValidationInput input)
        {
            foreach (var document in input.Documents)
            {
                var lineGeometries = document.GetGeometriesByType(GeoJsonGeometry.LineString, GeoJsonGeometry.MultiLineString);

                foreach (var indexed in lineGeometries)
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
