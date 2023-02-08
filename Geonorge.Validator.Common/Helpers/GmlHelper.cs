using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Geonorge.Validator.Common.Helpers
{
    public class GmlHelper
    {
        private static readonly Regex _dimensionsRegex = new(@"srsDimension=""(?<dimensions>(\d))""", RegexOptions.Compiled);
        private static readonly Regex _srsNameRegex = new(@"srsName=""(http:\/\/www\.opengis\.net\/def\/crs\/EPSG\/0\/|urn:ogc:def:crs:EPSG::|EPSG:)(?<epsg>\d+)""");

        public static async Task<int> GetDimensionsAsync(Stream stream)
        {
            stream.Position = 0;

            var buffer = new byte[50000];
            await stream.ReadAsync(buffer.AsMemory(0, 50000));

            stream.Position = 0;

            using var memoryStream = new MemoryStream(buffer);
            using var streamReader = new StreamReader(memoryStream);
            var gmlString = await streamReader.ReadToEndAsync();

            var dimensionsMatch = _dimensionsRegex.Match(gmlString);

            if (dimensionsMatch.Success && int.TryParse(dimensionsMatch.Groups["dimensions"].Value, out var dimensions))
                return dimensions;

            return 2;
        }

        public static async Task<int?> GetEpsgCodeAsync(IFormFile file)
        {
            var buffer = new byte[5000];
            await file.OpenReadStream().ReadAsync(buffer.AsMemory(0, 5000));

            using var memoryStream = new MemoryStream(buffer);
            using var streamReader = new StreamReader(memoryStream);
            var fileString = streamReader.ReadToEnd();
            var match = _srsNameRegex.Match(fileString);

            if (match == null)
                return null;

            if (!int.TryParse(match.Groups["epsg"].Value, out var code))
                return null;

            return code;
        }
    }
}
