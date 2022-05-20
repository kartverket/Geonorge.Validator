using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Utils
{
    public class GmlHelper
    {
        private static readonly Regex _dimensionsRegex = new(@"srsDimension=""(?<dimensions>(\d))""", RegexOptions.Compiled);

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
    }
}
