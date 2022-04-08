using Geonorge.XsdValidator.Config;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Utils
{
    public class XsdHelper
    {
        public static XmlSchemaSet CreateXmlSchemaSet(Stream xsdStream, XsdValidatorSettings settings)
        {
            if (xsdStream == null)
                return null;

            var xmlResolver = new XmlFileCacheResolver(settings);
            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = xmlResolver };
            var xmlSchema = XmlSchema.Read(xsdStream, null);

            xsdStream.Position = 0;
            xmlSchemaSet.Add(xmlSchema);

            try
            {
                xmlSchemaSet.Compile();
                SaveCachedXsdUris(xmlResolver.CachedUris, settings);

                return xmlSchemaSet;
            }
            catch (XmlSchemaException exception)
            {
                Log.Logger.Error(exception, "Kunne ikke kompilere XmlSchemaSet!");
                throw;
            }
        }

        private static void SaveCachedXsdUris(List<string> cachedUris, XsdValidatorSettings settings)
        {
            if (!cachedUris.Any())
                return;

            var filePath = Path.GetFullPath(Path.Combine(settings.CacheFilesPath, settings.CachedUrisFileName));
            var existingCachedUris = Array.Empty<string>();

            if (File.Exists(filePath))
                existingCachedUris = File.ReadAllLines(filePath);

            var union = existingCachedUris.Union(cachedUris);

            File.WriteAllLines(filePath, union);
        }
    }
}
