using Geonorge.Validator.Application.Models.Data;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Utils
{
    public class FileHelper
    {
        private static readonly Regex _xmlRegex = new(@"^<\?xml.*?<", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _gml32Regex = new(@"^<\?xml.*?<\w+:FeatureCollection.*?xmlns:\w+=""http:\/\/www\.opengis\.net\/gml\/3\.2""", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _xsdRegex = new(@"^<\?xml.*?<(.*:)?schema .*?xmlns(:.*)?=""http:\/\/www\.w3\.org\/2001\/XMLSchema""", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _jsonRegex = new(@"\A(\s*?({|\[))", RegexOptions.Compiled);

        public static async Task<FileType> GetFileType(MultipartSection section)
        {
            var buffer = new byte[500];
            await section.Body.ReadAsync(buffer.AsMemory(0, 500));
            section.Body.Position = 0;

            using var memoryStream = new MemoryStream(buffer);
            using var streamReader = new StreamReader(memoryStream);
            var fileString = streamReader.ReadToEnd();

            if (_xsdRegex.IsMatch(fileString))
                return FileType.XSD;

            if (_gml32Regex.IsMatch(fileString))
                return FileType.GML32;

            if (_xmlRegex.IsMatch(fileString))
                return FileType.XML;

            if (_jsonRegex.IsMatch(fileString))
                return FileType.JSON;

            return FileType.Unknown;
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
