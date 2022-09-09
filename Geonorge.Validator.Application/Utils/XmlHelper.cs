using Geonorge.Validator.Application.Exceptions;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.Utils
{
    public class XmlHelper
    {
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
    }
}
