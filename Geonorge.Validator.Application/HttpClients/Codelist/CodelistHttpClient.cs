using Geonorge.Validator.Application.Models.Data.Codelist;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Geonorge.Validator.Application.Utils.XsdHelpers;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistHttpClient : ICodelistHttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly CodelistSettings _settings;
        private readonly ILogger<CodelistHttpClient> _logger;
        public HttpClient Client { get; }

        public CodelistHttpClient(
            HttpClient client,
            IOptions<CodelistSettings> options,
            ILogger<CodelistHttpClient> logger)
        {
            Client = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<CodeSpace>> GetCodeSpaces(Stream xsdStream)
        {
            var codeSpaceDictionary = await GetGmlCodeSpacesFromXsd(xsdStream);

            if (!codeSpaceDictionary.Any())
                return new();

            var codelistTasks = codeSpaceDictionary
                .ToLookup(kvp => kvp.Value, kvp => kvp.Key)
                .Select(grouping => (Grouping: grouping, Task: FetchCodelist(grouping.Key)));

            await Task.WhenAll(codelistTasks.Select(task => task.Task));

            var codeSpaces = new List<CodeSpace>();

            foreach (var codelistTask in codelistTasks)
            {
                var codelist = await codelistTask.Task;

                if (codelist == null)
                    continue;

                var url = codelistTask.Grouping.Key;

                foreach (var xPath in codelistTask.Grouping)
                    codeSpaces.Add(new CodeSpace(xPath, url, codelist));
            }

            return codeSpaces;
        }

        public async Task<List<CodelistValue>> FetchCodelist(string url)
        {
            var uri = new Uri(url);

            if (!_settings.AllowedHosts.Contains(uri.Host))
                return null;

            try
            {
                return await FetchData(uri, DataResolver);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke laste ned kodeliste fra {url}", url);
                return null;
            }
        }

        private async Task<List<CodelistValue>> FetchData(Uri uri, Func<Stream, Task<List<CodelistValue>>> resolver)
        {
            var filePath = GetFilePath(uri);
            var data = await LoadDataFromDisk(filePath);

            if (data != null)
                return data;

            using var response = await Client.GetAsync(uri.AbsoluteUri);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();

            data = await resolver.Invoke(stream);

            await SaveDataToDisk(filePath, data);

            return data;
        }

        private readonly Func<Stream, Task<List<CodelistValue>>> DataResolver = async stream =>
        {
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, new CancellationToken());

            if (!IsCodelist(document))
                return null;

            return document.Root.Descendants("Registeritem")
                .Select(element =>
                {
                    return new CodelistValue(
                        element.Element("codevalue").Value,
                        element.Element("label")?.Value,
                        element.Element("description")?.Value
                    );
                })
                .ToList();
        };

        private string GetFilePath(Uri uri)
        {
            var path = Path.GetFullPath(Path.Combine(_settings.CacheFilesPath, uri.Host + uri.LocalPath));

            return Path.ChangeExtension(path, "json");
        }

        private async Task<List<CodelistValue>> LoadDataFromDisk(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var sinceLastUpdate = DateTime.Now.Subtract(File.GetLastWriteTime(filePath));

            if (sinceLastUpdate.TotalDays >= _settings.CacheDurationDays)
                return null;

            return JsonConvert.DeserializeObject<List<CodelistValue>>(await File.ReadAllTextAsync(filePath));
        }

        private static async Task SaveDataToDisk(string filePath, object data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
        }

        private static bool IsCodelist(XDocument document) => document.Root.Element("containedItemClass")?.Value == "CodelistValue";
    }
}
