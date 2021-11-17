using Geonorge.Validator.Application.Exceptions;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace Geonorge.Validator.Application.Utils
{
    public class XsdHelpers
    {
        public static async Task<(string XmlNamespace, string XsdVersion)> GetXmlNamespaceAndXsdVersion(Stream xsdStream)
        {
            var document = await LoadXDocumentAsync(xsdStream);

            return (
                document.Root.XPath2SelectOne<XAttribute>("@targetNamespace")?.Value,
                document.Root.XPath2SelectOne<XAttribute>("@version")?.Value
            );
        }

        public static async Task<Dictionary<string, string>> GetGmlCodeSpacesFromXsd(Stream xsdStream)
        {
            var document = await LoadXDocumentAsync(xsdStream);
            var codeTypeElements = document.XPath2SelectElements("//*[@type='gml:CodeType']");
            var xPaths = new Dictionary<string, string>();

            foreach (var element in codeTypeElements)
            {
                var defaultCodeSpace = element.XPath2SelectElement("//*:defaultCodeSpace")?.Value;

                if (string.IsNullOrWhiteSpace(defaultCodeSpace))
                    continue;

                var elementName = element.Attribute("name").Value;
                var complexTypeName = element.XPath2SelectOne<XAttribute>("ancestor::*:complexType[@name][1]/@name")?.Value;
                string xPath;

                if (complexTypeName != null)
                {
                    var featureElementName = document.XPath2SelectOne<XAttribute>($"//*:element[contains(@type, ':{complexTypeName}')]/@name")?.Value;
                    xPath = $"//*:{featureElementName}//*:{elementName}";
                }
                else
                {
                    xPath = $"//*:{elementName}";
                }

                xPaths.Add(xPath, defaultCodeSpace);
            }

            return xPaths;
        }

        private static async Task<XDocument> LoadXDocumentAsync(Stream xsdStream)
        {
            try
            {
                var document = await XDocument.LoadAsync(xsdStream, LoadOptions.None, new CancellationToken());
                xsdStream.Position = 0;

                return document;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Ugyldig applikasjonsskjema");
                throw new InvalidXsdException($"Ugyldig applikasjonsskjema");
            }
        }
    }
}
