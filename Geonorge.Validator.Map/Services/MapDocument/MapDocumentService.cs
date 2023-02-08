using Geonorge.Validator.Common.Helpers;
using Geonorge.Validator.Map.Exceptions;
using Geonorge.Validator.Map.Models.Config.Styling;
using Geonorge.Validator.Map.Models.Map;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Geonorge.Validator.Map.Services
{
    public class MapDocumentService : IMapDocumentService
    {
        private readonly IGmlToGeoJsonService _gmlToGeoJsonService;
        private readonly StylingSettings _stylingSettings;

        public MapDocumentService(
            IGmlToGeoJsonService gmlToGeoJsonService,
            IOptions<StylingSettings> options)
        {
            _gmlToGeoJsonService = gmlToGeoJsonService;
            _stylingSettings = options.Value;
        }

        public async Task<MapDocument> CreateMapDocumentAsync(IFormFile file)
        {
            var document = await XmlHelper.LoadXDocumentAsync(file.OpenReadStream());
            var projection = await GetProjectionAsync(file);

            var mapDocument = new MapDocument
            {
                FileName = file.FileName,
                FileSize = file.Length,
                Projection = projection,
                GeoJson = _gmlToGeoJsonService.CreateGeoJsonDocument(document, projection.AxisOrientation),
                Styling = GetMapStyling(file)
            };

            if (mapDocument.Projection == null)
                throw new MapDocumentException("GML-filen har ingen gyldig EPSG-kode.");

            if (!mapDocument.GeoJson.Features.Any())
                throw new MapDocumentException("GML-filen inneholder ingen gyldige features.");

            return mapDocument;
        }

        private MapStyling GetMapStyling(IFormFile file)
        {
            var @namespace = XmlHelper.GetDefaultNamespace(file);
            var mapStyling = _stylingSettings.Specifications.SingleOrDefault(specification => specification.Value.Namespace == @namespace);

            if (!mapStyling.Equals(default(KeyValuePair<string, MapStyling>)))
                return mapStyling.Value;

            return null;
        }

        private static async Task<Projection> GetProjectionAsync(IFormFile file)
        {
            var code = await GmlHelper.GetEpsgCodeAsync(file);

            return code.HasValue ? Projection.Create(code.Value) : null;
        }
    }
}
