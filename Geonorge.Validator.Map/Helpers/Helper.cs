using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Geonorge.Validator.Map.Helpers
{
    public class Helpers
    {
        private static readonly Regex _schemaLocationRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        public static string GetDefaultNamespace(IFormFile gmlFile)
        {
            var xmlString = ReadLines(gmlFile.OpenReadStream(), 50);
            var match = _schemaLocationRegex.Match(xmlString);

            if (!match.Success)
                return null;

            var values = match.Groups["schema_loc"].Value.Split(" ");

            return values.ElementAtOrDefault(0);
        }

        public static string ReadLines(Stream stream, int numberOfLines)
        {
            if (numberOfLines < 1)
                throw new ArgumentException("numberOfLines må være større enn 0");

            var counter = 0;
            var stringBuilder = new StringBuilder(numberOfLines * 250);
            using var streamReader = new StreamReader(stream);

            while (counter++ < numberOfLines && !streamReader.EndOfStream)
                stringBuilder.Append(streamReader.ReadLine());

            return stringBuilder.ToString();
        }
    }
}
