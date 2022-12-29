using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;
using static Geonorge.Validator.Common.Helpers.XmlHelper;
using CodeList = Geonorge.Validator.Application.Models.Data.Codelist.Codelist;
using Formatting = Newtonsoft.Json.Formatting;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistHttpClient : ICodelistHttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly CodelistSettings _settings;
        private readonly ILogger<CodelistHttpClient> _logger;
        private readonly List<string> _cachedUris;

        public CodelistHttpClient(
            HttpClient httpClient,
            IMemoryCache memoryCache,
            IOptions<CodelistSettings> options,
            ILogger<CodelistHttpClient> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _settings = options.Value;
            _logger = logger;
            _cachedUris = new();
        }

        public async Task<CodeList> GetCodelistAsync(Uri uri)
        {
            return await _memoryCache.GetOrCreateAsync(uri, async cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromMinutes(5);

                _cachedUris.Clear();

                var codeList = await FetchCodelistAsync(uri);
                await SaveCachedCodelistUrisAsync();

                return codeList;
            });
        }
        
        public async Task<int> UpdateCacheAsync(bool forceUpdate)
        {
            var cacheListFilePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));

            if (!File.Exists(cacheListFilePath))
                return 0;

            var lines = await File.ReadAllLinesAsync(cacheListFilePath);
            var tasks = new List<(Task<CodeList> Request, Uri uri, string FilePath)>();

            foreach (var line in lines)
            {
                var lineSplit = line.Split(",");
                var lastCached = DateTime.Parse(lineSplit[1]);

                if (!IsOutdated(lastCached, forceUpdate) || !Uri.TryCreate(lineSplit[0], UriKind.Absolute, out var uri))
                    continue;

                var filePath = GetFilePath(uri);
                var task = FetchDataAsync(uri);

                tasks.Add((task, uri, filePath));
            }

            _cachedUris.Clear();

            await Task.WhenAll(tasks.Select(task => task.Request));

            foreach (var (request, uri, filePath) in tasks)
            {
                var data = request.Result;

                if (data == null)
                    continue;

                await SaveDataToDiskAsync(filePath, data);

                _cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            }

            await SaveCachedCodelistUrisAsync();

            if (_memoryCache is MemoryCache memoryCache)
                memoryCache.Compact(1);

            return _cachedUris.Count;
        }

        private async Task<CodeList> FetchCodelistAsync(Uri uri)
        {
            if (!_settings.AllowedHosts.Contains(uri.Host))
                return null;

            var filePath = GetFilePath(uri);
            var data = await LoadDataFromDiskAsync(filePath);

            if (data != null)
                return data;

            data = await FetchDataAsync(uri);

            if (data == null)
                return null;

            await SaveDataToDiskAsync(filePath, data);

            _cachedUris.Add($"{uri.AbsoluteUri},{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");

            return data;
        }

        private async Task<CodeList> FetchDataAsync(Uri uri)
        {
            var codelist = new CodeList { Uri = uri };

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri.AbsoluteUri);

                if (!Path.HasExtension(uri.AbsoluteUri))
                    request.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/xml");

                using var response = await _httpClient.SendAsync(request);
                codelist.HttpStatusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    using var stream = await response.Content.ReadAsStreamAsync();

                    try
                    {
                        codelist.Items = await CreateCodelistAsync(stream);
                    }
                    catch
                    {
                        codelist.Status = CodelistStatus.InvalidCodelist;
                        return codelist;
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                    codelist.Status = CodelistStatus.CodelistNotFound;
                else if (response.StatusCode != HttpStatusCode.OK)
                    codelist.Status = CodelistStatus.CodelistUnavailable;
                else
                    codelist.Status = CodelistStatus.CodelistFound;

                return codelist;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke laste ned kodeliste fra {uri}", uri.AbsoluteUri);
                throw;
            }
        }

        private string GetFilePath(Uri uri)
        {
            var path = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));

            return $"{path}.json";
        }

        private async Task SaveCachedCodelistUrisAsync()
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

        private static async Task<CodeList> LoadDataFromDiskAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return JsonConvert.DeserializeObject<CodeList>(await File.ReadAllTextAsync(filePath));
        }

        private static async Task<List<CodelistItem>> CreateCodelistAsync(Stream stream)
        {
            var document = await LoadXDocumentAsync(stream);
            var codelistType = GetCodelistType(document);

            return codelistType switch
            {
                CodelistType.GeonorgeCodelist => MapFromGeonorgeCodelist(document),
                CodelistType.GmlDictionary => MapFromGmlDictionary(document),
                _ => null,
            };
        }

        private static async Task SaveDataToDiskAsync(string filePath, object data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
        }

        private static CodelistType GetCodelistType(XDocument document)
        {
            if (document.Root.Name == "Register" && document.Root.Element("containedItemClass")?.Value == "CodelistValue")
                return CodelistType.GeonorgeCodelist;

            if (document.Root.Name == "{http://www.opengis.net/gml/3.2}Dictionary")
                return CodelistType.GmlDictionary;

            return CodelistType.Unknown;
        }

        private static List<CodelistItem> MapFromGeonorgeCodelist(XDocument document)
        {
            return document.Root.XPath2SelectElements("*:containeditems/*:Registeritem")
                .Select(element =>
                {
                    return new CodelistItem(
                        element.XPath2SelectElement("*:label")?.Value,
                        element.XPath2SelectElement("*:codevalue")?.Value,
                        element.XPath2SelectElement("*:description")?.Value
                    );
                })
                .OrderBy(codelistItem => codelistItem.Name)
                .ToList();
        }

        private static List<CodelistItem> MapFromGmlDictionary(XDocument document)
        {
            return document.Root.XPath2SelectElements("*:dictionaryEntry")
                .Select(element =>
                {
                    return new CodelistItem(
                        element.XPath2SelectElement("//*:name")?.Value,
                        element.XPath2SelectElement("//*:identifier")?.Value,
                        element.XPath2SelectElement("//*:description")?.Value
                    );
                })
                .OrderBy(codelistItem => codelistItem.Name)
                .ToList();
        }

        private static bool IsOutdated(DateTime lastCached, bool forceUpdate)
        {
            if (forceUpdate)
                return true;

            return (DateTime.Now - lastCached).TotalHours >= 24;
        }
    }
}
