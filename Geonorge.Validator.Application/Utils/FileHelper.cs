using Geonorge.Validator.Application.Models.Data;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Utils
{
    public class FileHelper
    {
        private static readonly Regex _xmlRegex = new(@"^<\?xml.*?<", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _gml32Regex = new(@"^<\?xml.*?<\w+:FeatureCollection.*?xmlns:\w+=""http:\/\/www\.opengis\.net\/gml\/3\.2""", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex _xsdRegex = new(@"^<\?xml.*?<(.*:)?schema .*?xmlns(:.*)?=""http:\/\/www\.w3\.org\/2001\/XMLSchema""", RegexOptions.Compiled | RegexOptions.Singleline);

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

            return FileType.Unknown;
        }
    }
}
