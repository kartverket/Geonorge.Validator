using Geonorge.Validator.XmlSchema.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml;

namespace Geonorge.Validator.XmlSchema.Utils
{
    public class XmlFileCacheResolver : XmlUrlResolver
    {
        private readonly Uri _baseUri;
        private readonly XsdValidatorSettings _settings;
        private readonly HttpClient _client;

        public XmlFileCacheResolver(
            Uri baseUri,
            XsdValidatorSettings settings)
        {
            _baseUri = baseUri;
            _settings = settings;
            _client = new();
        }

        public List<string> CachedUris { get; } = new();

        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri == null)
                throw new ArgumentNullException(nameof(absoluteUri));

            var uri = ResolveUri(absoluteUri);

            if (uri.Scheme == "http" && (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream)))
            {
                var filePath = GetFilePath(uri);

                if (File.Exists(filePath))
                    return File.OpenRead(filePath);                   

                using var response = _client.GetAsync(uri).Result;
                var stream = response.Content.ReadAsStream();

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                if (ShouldCache(uri) && memoryStream.Length > 0)
                {
                    using var fileStream = CreateFile(filePath);
                    memoryStream.CopyTo(fileStream);
                    memoryStream.Position = 0;
                    CacheUri(uri.AbsoluteUri);
                }

                stream.Dispose();
                return memoryStream;
            }
            else
            {
                return base.GetEntity(uri, role, ofObjectToReturn);
            }
        }

        private Uri ResolveUri(Uri absoluteUri)
        {
            if (absoluteUri.Scheme == "file" && _baseUri != null)
                return new Uri(_baseUri, absoluteUri.Segments.Last());

            return absoluteUri;
        }

        private bool ShouldCache(Uri uri)
        {
            return _settings.CacheableHosts == null || _settings.CacheableHosts.Contains(uri.Host);
        }

        private void CacheUri(string uri)
        {
            CachedUris.Add($"{uri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));
        }

        private static FileStream CreateFile(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            return File.Create(filePath);
        }
    }
}
