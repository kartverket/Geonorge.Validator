using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Schema;
using _XmlSchema = System.Xml.Schema.XmlSchema;

namespace Geonorge.Validator.XmlSchema.Utils
{
    public class XmlSchemaHelper
    {
        public static XmlSchemaSet CreateXmlSchemaSet(XmlSchemaData xmlSchemaData, XmlSchemaValidatorSettings settings)
        {
            if (xmlSchemaData == null || !xmlSchemaData.Streams.Any())
                return null;

            var httpClient = new HttpClient();
            var xmlResolver = new XmlFileCacheResolver(httpClient, settings);
            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = xmlResolver };

            AddXmlSchemas(xmlSchemaSet, xmlSchemaData, settings);

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
            finally
            {
                httpClient.Dispose();
            }
        }

        private static void AddXmlSchemas(XmlSchemaSet xmlSchemaSet, XmlSchemaData xmlSchemaData, XmlSchemaValidatorSettings settings)
        {
            if (xmlSchemaData.SchemaUris.Any())
            {
                foreach (var schemaUri in xmlSchemaData.SchemaUris)
                {
                    var filePath = Path.GetFullPath(Path.Combine(settings.CacheFilesPath, schemaUri.Host + schemaUri.LocalPath));
                    using var stream = File.OpenRead(filePath);
                    var xmlSchema = _XmlSchema.Read(stream, null);
                    xmlSchemaSet.Add(xmlSchema);
                }
            }
            else
            {
                foreach (var stream in xmlSchemaData.Streams)
                {
                    var xmlSchema = _XmlSchema.Read(stream, null);
                    stream.Position = 0;
                    xmlSchemaSet.Add(xmlSchema);
                }
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
