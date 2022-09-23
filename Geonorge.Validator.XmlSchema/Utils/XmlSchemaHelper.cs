using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using _XmlSchema = System.Xml.Schema.XmlSchema;

namespace Geonorge.Validator.XmlSchema.Utils
{
    public class XmlSchemaHelper
    {
        public static XmlSchemaSet CreateXmlSchemaSet(XmlSchemaData xsdData, XmlSchemaValidatorSettings settings)
        {
            if (xsdData == null || !xsdData.Streams.Any())
                return null;

            var xmlResolver = new XmlFileCacheResolver(xsdData.BaseUri, settings);
            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = xmlResolver };

            foreach (var stream in xsdData.Streams)
            {
                var xmlSchema = _XmlSchema.Read(stream, null);
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

        private static void SaveCachedXsdUris(List<string> cachedUris, XmlSchemaValidatorSettings settings)
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
