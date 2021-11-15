using Geonorge.XsdValidator.Config;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;

namespace Geonorge.XsdValidator.Utils
{
    public class XmlFileCacheResolver : XmlUrlResolver
    {
        private readonly XsdValidatorSettings _options;
        private readonly HttpClient _client;

        public XmlFileCacheResolver(
            XsdValidatorSettings options)
        {
            _options = options;
            _client = new();
        }

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri == null)
                throw new ArgumentNullException(nameof(absoluteUri));

            if (absoluteUri.Scheme == "http" && (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream)))
            {
                var filePath = GetFilePath(absoluteUri);

                if (File.Exists(filePath))
                {
                    var lastUpdated = DateTime.Now.Subtract(File.GetLastWriteTime(filePath));

                    if (lastUpdated.TotalDays < _options.CacheDurationDays)
                        return File.OpenRead(filePath);                   
                }

                using var response = _client.GetAsync(absoluteUri).Result;
                var stream = response.Content.ReadAsStream();

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                if (ShouldCache(absoluteUri) && memoryStream.Length > 0)
                {
                    using var fileStream = CreateFile(filePath);
                    memoryStream.CopyTo(fileStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                }

                stream.Dispose();
                return memoryStream;
            }
            else
            {
                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
        }

        private bool ShouldCache(Uri uri)
        {
            return _options.CacheableHosts == null || _options.CacheableHosts.Contains(uri.Host);
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_options.CacheFilesPath, uri.Host + uri.LocalPath));
        }

        private static FileStream CreateFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            return File.Create(filePath);
        }
    }
}
