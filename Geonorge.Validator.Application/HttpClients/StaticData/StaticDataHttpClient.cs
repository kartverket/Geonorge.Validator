using DiBK.RuleValidator.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reguleringsplanforslag.Rules.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Geonorge.Validator.Application.HttpClients.StaticData.StaticDataSettings;

namespace Geonorge.Validator.Application.HttpClients.StaticData
{
    public class StaticDataHttpClient : IStaticDataHttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly StaticDataSettings _settings;
        private readonly ILogger<StaticDataHttpClient> _logger;
        public HttpClient Client { get; }

        public StaticDataHttpClient(
            HttpClient client,
            IOptions<StaticDataSettings> options,
            ILogger<StaticDataHttpClient> logger)
        {
            Client = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<GmlDictionaryEntry>> GetArealformål()
        {
            return await GetData(_settings.Arealformål, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:dictionaryEntry/*:Definition")
                    .Select(element => new GmlDictionaryEntry(element.GetValue("*:name"), element.GetValue("*:identifier")))
                    .OrderBy(arealformål => arealformål.Identifier)
                    .ToList();
            });
        }

        public async Task<List<GmlDictionaryEntry>> GetFeltnavnArealformål()
        {
            return await GetData(_settings.FeltnavnArealformål, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:dictionaryEntry/*:Definition")
                    .Select(element => new GmlDictionaryEntry(element.GetValue("*:name"), element.GetValue("*:identifier"), element.GetValue("*:description")))
                    .OrderBy(feltnavn => feltnavn.Identifier)
                    .ToList();
            });
        }

        public async Task<List<GeonorgeCodelistValue>> GetHensynskategori()
        {
            return await GetData(_settings.Hensynskategori, stream =>
            {
                return XDocument.Load(stream)
                    .GetElements("//*:containeditems/*:Registeritem")
                    .Select(element => new GeonorgeCodelistValue(element.GetValue("*:label"), element.GetValue("*:description"), element.GetValue("*:codevalue")))
                    .OrderBy(feltnavn => feltnavn.Codevalue)
                    .ToList();
            });
        }

        private async Task<T> GetData<T>(DataSource source, Func<Stream, T> resolver) where T : class
        {
            var filePath = Path.Combine(_settings.CacheFilesPath, source.FileName);
            var data = await LoadDataFromDisk<T>(filePath, source.CacheDays);

            if (data != null)
                return data;

            try
            {
                using var response = await Client.GetAsync(source.Url);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();

                data = resolver.Invoke(stream);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke laste ned data fra {sourceUrl}!", source.Url);
                return null;
            }

            await SaveDataToDisk(filePath, data);

            return data;
        }

        private static async Task SaveDataToDisk(string filePath, object data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            await File.WriteAllTextAsync(filePath, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
        }

        private static async Task<T> LoadDataFromDisk<T>(string filePath, int cacheDurationDays) where T : class
        {
            if (!File.Exists(filePath))
                return null;

            var sinceLastUpdate = DateTime.Now.Subtract(File.GetLastWriteTime(filePath));

            if (sinceLastUpdate.TotalDays >= cacheDurationDays)
                return null;

            return JsonConvert.DeserializeObject<T>(await File.ReadAllTextAsync(filePath));
        }
    }
}
