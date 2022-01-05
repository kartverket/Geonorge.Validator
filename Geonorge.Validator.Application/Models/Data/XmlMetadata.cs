using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;
using Geonorge.Validator.Application.Utils;

namespace Geonorge.Validator.Application.Models.Data
{
    public class XmlMetadata
    {
        public string Namespace { get; private set; }
        public string XsdVersion { get; private set; }
        public string GmlVersion { get; private set; }
        public bool IsGml32 => GmlVersion?.StartsWith("3.2") ?? false;

        private XmlMetadata(string @namespace, string xsdVersion, string gmlVersion)
        {
            Namespace = @namespace;
            XsdVersion = xsdVersion;
            GmlVersion = gmlVersion;
        }

        public static async Task<XmlMetadata> CreateAsync(Stream xsdStream, string xsdCacheFilesPath)
        {
            var document = await XmlHelper.LoadXDocumentAsync(xsdStream);

            return new XmlMetadata(
                document.Root.Attribute("targetNamespace")?.Value,
                document.Root.Attribute("version")?.Value,
                await GetGmlVersionAsync(document, xsdCacheFilesPath)
            );
        }

        private static async Task<string> GetGmlVersionAsync(XDocument document, string xsdCacheFilesPath)
        {
            var ns = document.Root.GetNamespaceOfPrefix("gml");

            if (ns == null)
                return null;

            var uriString = document.Root.XPath2SelectOne<XAttribute>($"//*:import[@namespace = '{ns.NamespaceName}']/@schemaLocation")?.Value ?? "";

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                return null;

            try
            {
                var filePath = Path.GetFullPath(Path.Combine(xsdCacheFilesPath, uri.Host + uri.LocalPath));
                using var stream = File.OpenRead(filePath);
                var gmlXsdDocument = await XmlHelper.LoadXDocumentAsync(stream);

                return gmlXsdDocument.Root.Attribute("version")?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}
