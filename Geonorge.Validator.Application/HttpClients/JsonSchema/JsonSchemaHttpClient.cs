using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Geonorge.Validator.Common.Helpers.FileHelper;

namespace Geonorge.Validator.Application.HttpClients.JsonSchema
{
    public class JsonSchemaHttpClient : IJsonSchemaHttpClient
    {
        private static readonly Regex _jsonSchemaRegex = new(@"""\$schema"":\s*""(?<uri>(.*?))""", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly JsonSchemaValidatorSettings _settings;
        private readonly ILogger<JsonSchemaHttpClient> _logger;

        public JsonSchemaHttpClient(
            HttpClient client,
            IOptions<JsonSchemaValidatorSettings> options,
            ILogger<JsonSchemaHttpClient> logger)
        {
            _httpClient = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<JSchema> GetJsonSchemaAsync(DisposableList<InputData> inputData, Stream schemaStream)
        {
            try
            {
                Uri baseUri = null;

                if (schemaStream == null)
                {
                    var schemaUri = GetSchemaUriFromJsonFiles(inputData);
                    baseUri = new Uri(schemaUri.GetLeftPart(UriPartial.Authority));
                    schemaStream = await FetchJsonSchemaAsync(schemaUri);
                }

                using var jsonReader = new JsonTextReader(new StreamReader(schemaStream));
                var resolver = new JsonSchemaUrlResolver(_httpClient, baseUri, _settings);
                
                var schema = JSchema.Load(jsonReader, resolver);
                await SaveCachedJsonSchemaUris(resolver.CachedUris, _settings);

                return schema;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke laste JSON-skjema");
                throw new InvalidJsonSchemaException("Kunne ikke laste JSON-skjema");
            }
        }

        public async Task<int> UpdateCacheAsync(bool forceUpdate)
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

                if (!IsOutdated(lastCached, forceUpdate) || !Uri.TryCreate(lineSplit[0], UriKind.Absolute, out var uri))
                    continue;

                var filePath = GetFilePath(uri);
                var task = FetchJsonSchemaAsync(uri);

                tasks.Add((task, uri, filePath));
            }

            var cachedUris = new List<string>();

            await Task.WhenAll(tasks.Select(task => task.Request));

            foreach (var (request, uri, filePath) in tasks)
            {
                var data = request.Result;

                if (data == null)
                    continue;

                await SaveDataToDiskAsync(data, filePath);

                cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            }

            await SaveCachedCodelistUrisAsync(cachedUris);

            return cachedUris.Count;
        }

        private async Task<MemoryStream> FetchJsonSchemaAsync(Uri schemaUri)
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
                throw new InvalidJsonSchemaException($"Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.");
            }
        }

        private string GetFilePath(Uri uri)
        {
            return Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));
        }

        private async Task SaveCachedCodelistUrisAsync(List<string> cachedUris)
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

        private static async Task SaveDataToDiskAsync(MemoryStream memoryStream, string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await memoryStream.CopyToAsync(fileStream);
            await memoryStream.DisposeAsync();
        }

        private static async Task SaveCachedJsonSchemaUris(List<string> cachedUris, JsonSchemaValidatorSettings settings)
        {
            if (!cachedUris.Any())
                return;

            var filePath = Path.GetFullPath(Path.Combine(settings.CacheFilesPath, settings.CachedUrisFileName));
            var existingCachedUris = Array.Empty<string>();

            if (File.Exists(filePath))
                existingCachedUris = await File.ReadAllLinesAsync(filePath);

            var union = existingCachedUris.Union(cachedUris);

            await File.WriteAllLinesAsync(filePath, union);
        }

        private static Uri GetSchemaUriFromJsonFiles(DisposableList<InputData> inputData)
        {
            var schemaUris = new List<string>();

            foreach (var data in inputData)
            {
                var json = ReadLines(data.Stream, 50);
                var match = _jsonSchemaRegex.Match(json);

                if (!match.Success)
                    continue;

                schemaUris.Add(match.Groups["uri"].Value);
            }

            if (!schemaUris.Any())
                throw new InvalidJsonSchemaException("Filene i datasettet mangler applikasjonsskjema.");

            if (schemaUris.Count != inputData.Count || schemaUris.Distinct().Count() != 1)
                throw new InvalidJsonSchemaException("Filene i datasettet har ulike applikasjonsskjemaer.");

            var uriString = schemaUris.First();

            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                return uri;

            throw new InvalidJsonSchemaException($"Datasettet har en ugyldig skjema-URI {uriString}");
        }

        private static bool IsOutdated(DateTime lastCached, bool forceUpdate)
        {
            if (forceUpdate)
                return true;

            return (DateTime.Now - lastCached).TotalHours >= 24;
        }
    }
}
