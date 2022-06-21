using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Models;
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
        public static XmlSchemaSet CreateXmlSchemaSet(XsdData xsdData, XsdValidatorSettings settings)
        {
            if (xsdData == null || !xsdData.Streams.Any())
                return null;

            var xmlResolver = new XmlFileCacheResolver(xsdData.BaseUri, settings);
            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = xmlResolver };

            foreach (var stream in xsdData.Streams)
            {
                var xmlSchema = XmlSchema.Read(stream, null);
                stream.Position = 0;
                xmlSchemaSet.Add(xmlSchema);
            }

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
