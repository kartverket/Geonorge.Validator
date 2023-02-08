using Geonorge.Validator.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Geonorge.Validator.Common.Helpers
{
    public class XmlHelper
    {
        private static readonly Regex _schemaLocationRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        public static async Task<XDocument> LoadXDocumentAsync(Stream xmlStream, LoadOptions loadOptions = LoadOptions.None)
        {
            try
            {
                var document = await XDocument.LoadAsync(xmlStream, loadOptions, default);
                xmlStream.Position = 0;

                return document;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Ugyldig XML-dokument");
                throw new InvalidXmlSchemaException($"Ugyldig XML-dokument");
            }
        }

        public static string GetDefaultNamespace(IFormFile xmlFile)
        {
            var xmlString = FileHelper.ReadLines(xmlFile.OpenReadStream(), 50);
            var match = _schemaLocationRegex.Match(xmlString);

            if (!match.Success)
                return null;

            var values = match.Groups["schema_loc"].Value.Split(" ");

            return values.ElementAtOrDefault(0);
        }

        public static XElement GetElementAtLine(XDocument document, int lineNumber)
        {
            return document.Descendants()
                .SingleOrDefault(element => ((IXmlLineInfo)element).LineNumber == lineNumber);
        }

        public static XElement GetElementAtLine(XDocument document, int lineNumber, int linePosition)
        {
            return document.Descendants()
                .SingleOrDefault(element =>
                {
                    var lineInfo = (IXmlLineInfo)element;
                    return lineInfo.LineNumber == lineNumber && lineInfo.LinePosition == linePosition;
                });
        }
    }
}
