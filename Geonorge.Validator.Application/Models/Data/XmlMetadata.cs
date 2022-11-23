using Geonorge.Validator.Common.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlMetadata
    {
        private static readonly Regex _gml32NsRegex = new(@"xmlns:gml=""http:\/\/www\.opengis\.net\/gml\/3\.2""", RegexOptions.Compiled);

        public List<(string Namespace, string XsdVersion)> Namespaces { get; private set; }
        public bool IsGml32 { get; private set; }

        private XmlMetadata(List<(string Namespace, string XsdVersion)> namespaces, bool isGml32)
        {
            Namespaces = namespaces;
            IsGml32 = isGml32;
        }

        public static async Task<XmlMetadata> CreateAsync(Stream xmlStream, List<Stream> xsdStreams)
        {
            var namespaces = new List<(string Namespace, string XsdVersion)>();

            foreach (var stream in xsdStreams)
            {
                var document = await XmlHelper.LoadXDocumentAsync(stream);
                namespaces.Add((document.Root.Attribute("targetNamespace")?.Value, document.Root.Attribute("version")?.Value));
            }

            return new XmlMetadata(namespaces, HasGml32Namespace(xmlStream));
        }

        private static bool HasGml32Namespace(Stream xmlStream)
        {
            var contents = FileHelper.ReadLines(xmlStream, 50);

            return _gml32NsRegex.IsMatch(contents);
        }
    }
}
