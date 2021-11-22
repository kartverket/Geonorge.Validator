using Geonorge.XsdValidator.Config;
using Serilog;
using System.IO;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Utils
{
    public class XsdHelper
    {
        public static XmlSchemaSet CreateXmlSchemaSet(Stream xsdStream, XsdValidatorSettings settings)
        {
            if (xsdStream == null)
                return null;

            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = new XmlFileCacheResolver(settings) };
            var xmlSchema = XmlSchema.Read(xsdStream, null);
            xsdStream.Seek(0, SeekOrigin.Begin);

            xmlSchemaSet.Add(xmlSchema);

            return CompileSchemaSet(xmlSchemaSet);
        }

        private static XmlSchemaSet CompileSchemaSet(XmlSchemaSet xmlSchemaSet)
        {
            try
            {
                xmlSchemaSet.Compile();
                return xmlSchemaSet;
            }
            catch (XmlSchemaException exception)
            {
                Log.Logger.Error(exception, "Could not compile XmlSchemaSet!");
                throw;
            }
        }
    }
}
