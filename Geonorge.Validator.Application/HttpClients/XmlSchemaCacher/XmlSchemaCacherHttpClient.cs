﻿using Geonorge.Validator.XmlSchema.Config;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.HttpClients.XmlSchemaCacher
{
    public class XmlSchemaCacherHttpClient : IXmlSchemaCacherHttpClient
    {
        private readonly HttpClient _client;
        private readonly XmlSchemaValidatorSettings _settings;
        private readonly List<string> _cachedUris;

        public XmlSchemaCacherHttpClient(
            HttpClient client,
            IOptions<XmlSchemaValidatorSettings> options)
        {
            _client = client;
            _settings = options.Value;
            _cachedUris = new();
        }

        public async Task CacheSchemasAsync(Uri uri)
        {
            var downloaded = new Dictionary<Uri, XDocument>();

            await DownloadSchemasAsync(uri, downloaded);

            UpdateSchemaLocations(downloaded);

            await SaveCachedXmlSchemaUrisAsync();
        }

        public async Task<int> UpdateCacheAsync(bool forceUpdate = false)
        {
            var cacheListFilePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));

            if (!File.Exists(cacheListFilePath))
                return 0;

            var lines = await File.ReadAllLinesAsync(cacheListFilePath);
            var tasks = new List<(Task<XDocument> Request, Uri uri)>();

            foreach (var line in lines)
            {
                var lineSplit = line.Split(",");
                var lastCached = DateTime.Parse(lineSplit[1]);

                if (!IsOutdated(lastCached, forceUpdate) || !Uri.TryCreate(lineSplit[0], UriKind.Absolute, out var uri))
                    continue;

                var task = DownloadSchemaAsync(uri);

                tasks.Add((task, uri));
            }

            await Task.WhenAll(tasks.Select(task => task.Request));            
            
            var downloaded = new Dictionary<Uri, XDocument>();
            _cachedUris.Clear();

            foreach (var (request, uri) in tasks)
            {
                var data = await request;

                if (data == null)
                    continue;

                _cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
                downloaded.Add(uri, data);
            }

            UpdateSchemaLocations(downloaded);

            await SaveCachedXmlSchemaUrisAsync();

            return _cachedUris.Count;
        }

        private async Task DownloadSchemasAsync(Uri uri, Dictionary<Uri, XDocument> downloaded)
        {
            if (!ShouldDownload(uri, downloaded))
                return;

            var document = await DownloadSchemaAsync(uri);
            downloaded.Add(uri, document);

            _cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");

            var schemaLocationElements = document.Root.Elements()
                .Where(element => element.Name.LocalName == "include" || element.Name.LocalName == "import")
                .ToList();

            var uris = GetUrisFromSchemaLocation(schemaLocationElements, uri);

            foreach (var schemaUri in uris)
                await DownloadSchemasAsync(schemaUri, downloaded);
        }

        private async Task<XDocument> DownloadSchemaAsync(Uri uri)
        {
            try
            {
                using var response = await _client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                return await XDocument.LoadAsync(stream, LoadOptions.None, default);
            }
            catch
            {
                return null;
            }
        }

        private void UpdateSchemaLocations(Dictionary<Uri, XDocument> downloaded)
        {
            foreach (var (uri, document) in downloaded)
            {
                var importElements = document.Root.Elements()
                    .Where(element => element.Name.LocalName == "import")
                    .ToList();

                UpdateSchemaLocations(importElements, uri);

                var includeElements = document.Root.Elements()
                    .Where(element => element.Name.LocalName == "include")
                    .ToList();

                UpdateSchemaLocations(includeElements, uri);

                SaveSchema(uri, document);
            }
        }

        private async Task SaveCachedXmlSchemaUrisAsync()
        {
            if (!_cachedUris.Any())
                return;

            var filePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));
            var existingCachedUris = Array.Empty<string>();

            if (File.Exists(filePath))
                existingCachedUris = await File.ReadAllLinesAsync(filePath);

            var union = _cachedUris.UnionBy(existingCachedUris, uri => uri.Split(',')[0]);

            await File.WriteAllLinesAsync(filePath, union);
        }

        private bool ShouldDownload(Uri uri, Dictionary<Uri, XDocument> downloaded)
        {
            var filePath = GetFilePath(uri);

            return !downloaded.ContainsKey(uri) && (!File.Exists(filePath) || !_settings.CacheableHosts.Contains(uri.Host));
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));
        }

        private void SaveSchema(Uri uri, XDocument document)
        {
            var filePath = GetFilePath(uri);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                document.Save(filePath);
            }
            catch
            {
            }
        }

        private static List<Uri> GetUrisFromSchemaLocation(List<XElement> elements, Uri baseUri)
        {
            var uris = new List<Uri>();

            foreach (var element in elements)
            {
                var schemaLocation = element.Attributes()
                    .SingleOrDefault(attribute => attribute.Name.LocalName == "schemaLocation");

                if (schemaLocation == null)
                    continue;

                if (Uri.TryCreate(schemaLocation.Value, UriKind.Absolute, out var resultUri) &&
                    (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps))
                {
                    uris.Add(resultUri);
                }
                else if (Uri.TryCreate(baseUri, schemaLocation.Value, out resultUri))
                {
                    uris.Add(resultUri);
                }
            }

            return uris;
        }

        private static void UpdateSchemaLocations(List<XElement> elements, Uri baseUri)
        {
            foreach (var element in elements)
            {
                var schemaLocation = element.Attributes()
                    .SingleOrDefault(attribute => attribute.Name.LocalName == "schemaLocation");

                if (schemaLocation == null || IsUrl(schemaLocation.Value))
                    continue;

                if (Uri.TryCreate(baseUri, schemaLocation.Value, out var resultUri))
                    schemaLocation.Value = resultUri.AbsoluteUri;
            }
        }

        private static bool IsUrl(string uriString)
        {
            return Uri.TryCreate(uriString, UriKind.Absolute, out var resultUri) &&
                (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps);
        }

        private static bool IsOutdated(DateTime lastCached, bool forceUpdate)
        {
            if (forceUpdate)
                return true;

            return (DateTime.Now - lastCached).TotalHours >= 24;
        }
    }
}
