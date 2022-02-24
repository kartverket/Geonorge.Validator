using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Utils.Codelist;
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
using static Geonorge.Validator.Application.Utils.XmlHelper;
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
        private readonly IXsdCodelistExtractor _xsdCodelistExtractor;
        private readonly CodelistSettings _settings;
        private readonly ILogger<CodelistHttpClient> _logger;
        private readonly List<string> _cachedUris = new();

        public CodelistHttpClient(
            HttpClient httpClient,
            IXsdCodelistExtractor xsdCodelistExtractor,
            IOptions<CodelistSettings> options,
            ILogger<CodelistHttpClient> logger)
        {
            _httpClient = httpClient;
            _xsdCodelistExtractor = xsdCodelistExtractor;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<CodeSpace>> GetCodeSpacesAsync(
            Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            _cachedUris.Clear();

            var codelistData = await GetCodelistDataAsync(xsdStream, xmlStreams, codelistSelectors);

            if (codelistData == null)
                return new();

            var codeSpaces = new List<CodeSpace>();

            foreach (var (uriAndXPaths, httpRequest) in codelistData)
            {
                var codelist = await httpRequest;

                if (codelist == null)
                    continue;

                var url = uriAndXPaths.Key.AbsoluteUri;

                foreach (var xPath in uriAndXPaths)
                    codeSpaces.Add(new CodeSpace(xPath, url, codelist));
            }

            await SaveCachedCodelistUrisAsync();

            return codeSpaces;
        }

        public async Task<List<GmlCodeSpace>> GetGmlCodeSpacesAsync(
            Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            _cachedUris.Clear();

            var codelistData = await GetCodelistDataAsync(xsdStream, xmlStreams, codelistSelectors);

            if (codelistData == null)
                return new();

            var gmlCodeSpaces = new List<GmlCodeSpace>();

            foreach (var (uriAndXPaths, httpRequest) in codelistData)
            {
                var codelist = httpRequest.Result;

                if (codelist == null)
                    continue;

                var url = uriAndXPaths.Key.AbsoluteUri;

                foreach (var fullXPath in uriAndXPaths)
                {
                    var (featureMemberName, xPath) = GetFeatureMemberNameWithXPathFromXPath(fullXPath);

                    var gmlCodeSpace = gmlCodeSpaces
                        .SingleOrDefault(gmlCodeSpace => gmlCodeSpace.FeatureMemberName == featureMemberName);

                    if (gmlCodeSpace != null)
                    {
                        gmlCodeSpace.CodeSpaces.Add(new CodeSpace(xPath, url, codelist));
                    }
                    else
                    {
                        gmlCodeSpace = new(featureMemberName);
                        gmlCodeSpace.CodeSpaces.Add(new CodeSpace(xPath, url, codelist));
                        gmlCodeSpaces.Add(gmlCodeSpace);
                    }
                }
            }

            await SaveCachedCodelistUrisAsync();

            return gmlCodeSpaces;
        }

        public async Task<List<CodelistItem>> GetCodelistAsync(Uri uri)
        {
            _cachedUris.Clear();

            var codeList = await FetchCodelistAsync(uri);

            await SaveCachedCodelistUrisAsync();

            return codeList;
        }

        public async Task<int> UpdateCacheAsync()
        {
            var cacheListFilePath = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, _settings.CachedUrisFileName));

            if (!File.Exists(cacheListFilePath))
                return 0;

            var lines = await File.ReadAllLinesAsync(cacheListFilePath);
            var tasks = new List<(Task<List<CodelistItem>> Request, Uri uri, string FilePath)>();

            foreach (var line in lines)
            {
                var lineSplit = line.Split(",");
                var lastCached = DateTime.Parse(lineSplit[1]);

                if ((DateTime.Now - lastCached).TotalHours < 24 || !Uri.TryCreate(lineSplit[0], UriKind.Absolute, out var uri))
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

            return _cachedUris.Count;
        }

        private async Task<IEnumerable<(IGrouping<Uri, string> uriAndXPaths, Task<List<CodelistItem>> httpRequest)>> GetCodelistDataAsync(
            Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors)
        {
            var codelistUris = await _xsdCodelistExtractor.GetCodelistUrisAsync(xsdStream, xmlStreams, codelistSelectors);

            if (!codelistUris.Any())
                return null;

            var codelistData = codelistUris
                .ToLookup(kvp => kvp.Value, kvp => kvp.Key)
                .Select(grouping => (Grouping: grouping, Task: FetchCodelistAsync(grouping.Key)));

            await Task.WhenAll(codelistData.Select(task => task.Task));

            return codelistData;
        }

        private async Task<List<CodelistItem>> FetchCodelistAsync(Uri uri)
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

        private async Task<List<CodelistItem>> FetchDataAsync(Uri uri)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, uri.AbsoluteUri);

                if (!Path.HasExtension(uri.AbsoluteUri))
                    request.Headers.Add(HttpRequestHeader.Accept.ToString(), "application/xml");

                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();

                return await CreateCodelistAsync(stream);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke laste ned kodeliste fra {uri}", uri.AbsoluteUri);
                return null;
            }
        }

        private string GetFilePath(Uri uri)
        {
            var path = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));

            return Path.ChangeExtension(path, "json");
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

        private static async Task<List<CodelistItem>> LoadDataFromDiskAsync(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            return JsonConvert.DeserializeObject<List<CodelistItem>>(await File.ReadAllTextAsync(filePath));
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

        private static (string FeatureMemberName, string XPath) GetFeatureMemberNameWithXPathFromXPath(string xPath)
        {
            const string featureMemberPath = "*:FeatureCollection/*:featureMember/";

            var restPath = xPath[featureMemberPath.Length..];
            var elementNames = restPath.Split("/");
            var featureMemberName = elementNames[0].TrimStart("*:".ToCharArray());
            var elementPath = string.Join("/", elementNames.Skip(1));

            return (featureMemberName, elementPath);
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
    }
}
