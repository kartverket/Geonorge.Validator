using Geonorge.Validator.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Geonorge.Validator.Application.Utils;
using Microsoft.Extensions.Options;
using Geonorge.XsdValidator.Config;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public class XsdHttpClient : IXsdHttpClient
    {
        private static readonly Regex _schemaLocationRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly XsdValidatorSettings _settings;
        private readonly ILogger<XsdHttpClient> _logger;

        public XsdHttpClient(
            HttpClient client,
            IOptions<XsdValidatorSettings> options,
            ILogger<XsdHttpClient> logger)
        {
            _httpClient = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<MemoryStream> GetXsdFromXmlFilesAsync(List<IFormFile> xmlFiles)
        {
            var schemaUri = GetSchemaUriFromXmlFiles(xmlFiles);

            return await FetchXsdAsync(schemaUri);
        }

        public async Task<int> UpdateCacheAsync()
        {
            var cacheListFilePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));

            if (!File.Exists(cacheListFilePath))
                return 0;

            var lines = await File.ReadAllLinesAsync(cacheListFilePath);
            var tasks = new List<(Task<MemoryStream> Request, Uri uri, string FilePath)>();

            foreach (var line in lines)
            {
                var lineSplit = line.Split(",");
                var lastCached = DateTime.Parse(lineSplit[1]);

                if ((DateTime.Now - lastCached).TotalHours < 24 || !Uri.TryCreate(lineSplit[0], UriKind.Absolute, out var uri))
                    continue;

                var filePath = GetFilePath(uri);
                var task = FetchXsdAsync(uri.AbsoluteUri);

                tasks.Add((task, uri, filePath));
            }

            await Task.WhenAll(tasks.Select(task => task.Request));
            var cachedUris = new List<string>();

            foreach (var (request, uri, filePath) in tasks)
            {
                var data = await request;

                if (data == null)
                    continue;

                await SaveXsdToDiskAsync(filePath, data);

                cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            }

            await SaveCachedXsdUrisAsync(cachedUris);

            return cachedUris.Count;
        }

        private async Task<MemoryStream> FetchXsdAsync(string schemaUri)
        {
            try
            {
                using var response = await _httpClient.GetAsync(schemaUri);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.", schemaUri);
                throw new InvalidXsdException($"Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.");
            }
        }

        private async Task SaveCachedXsdUrisAsync(List<string> cachedUris)
        {
            if (!cachedUris.Any())
                return;

            var filePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));
            var existingCachedUris = Array.Empty<string>();

            if (File.Exists(filePath))
                existingCachedUris = await File.ReadAllLinesAsync(filePath);

            var union = cachedUris.UnionBy(existingCachedUris, uri => uri.Split(',')[0]);

            await File.WriteAllLinesAsync(filePath, union);
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));
        }

        private static string GetSchemaUriFromXmlFiles(List<IFormFile> xmlFiles)
        {
            var schemaUris = xmlFiles
                .Select(xmlFile =>
                {
                    var xmlString = XmlHelper.ReadLines(xmlFile.OpenReadStream(), 10);
                    var match = _schemaLocationRegex.Match(xmlString);

                    if (!match.Success)
                        return null;

                    return match.Groups["schema_loc"].Value.Split(" ").ElementAtOrDefault(1);
                })
                .ToList();

            if (schemaUris.Count != xmlFiles.Count || schemaUris.Distinct().Count() > 1)
                throw new InvalidXsdException("Filene i datasettet har ulike applikasjonsskjemaer.");

            var schemaUri = schemaUris.FirstOrDefault();

            if (schemaUri == null)
                throw new InvalidXsdException("Filene i datasettet mangler applikasjonsskjema.");

            return schemaUri;
        }

        private static async Task SaveXsdToDiskAsync(string filePath, MemoryStream memoryStream)
        {
            var bytes = memoryStream.ToArray();
            using var fileStream = File.Open(filePath, FileMode.Create);
            await fileStream.WriteAsync(bytes);
        }
    }
}
