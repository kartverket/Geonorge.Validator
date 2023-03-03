using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using Geonorge.Validator.Common.Exceptions;
using Newtonsoft.Json.Schema;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace Geonorge.Validator.Application.Utils
{
    public class JsonSchemaUrlResolver : JSchemaResolver
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _baseUri;
        private readonly JsonSchemaValidatorSettings _settings;

        public JsonSchemaUrlResolver(
            HttpClient httpClient, 
            Uri baseUri,
            JsonSchemaValidatorSettings settings)
        {
            _httpClient = httpClient;
            _baseUri = baseUri;
            _settings = settings;
        }

        public List<string> CachedUris { get; } = new();

        public override Stream GetSchemaResource(ResolveSchemaContext context, SchemaReference reference)
        {
            if (!reference.BaseUri.IsAbsoluteUri && _baseUri == null)
                return null;

            Uri schemaUri = reference.BaseUri.IsAbsoluteUri ?
                reference.BaseUri :
                new Uri(_baseUri, reference.BaseUri);

            var filePath = GetFilePath(schemaUri);

            if (File.Exists(filePath))
                return File.OpenRead(filePath);

            try
            {
                using var stream = _httpClient.GetStreamAsync(schemaUri).Result;

                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                if (ShouldCache(schemaUri) && memoryStream.Length > 0)
                {
                    using var fileStream = CreateFile(filePath);
                    memoryStream.CopyTo(fileStream);
                    memoryStream.Position = 0;
                    CacheUri(schemaUri.AbsoluteUri);
                }

                return memoryStream;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, $"Kunne ikke laste ned JSON-skjema fra {schemaUri}");
                throw new InvalidJsonSchemaException($"Kunne ikke laste ned JSON-skjema fra {schemaUri}");
            }
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
