using Geonorge.Validator.Application.Models.Geonorge;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.GmlApplicationSchemaRegistry
{
    public class GmlApplicationSchemaRegistryHttpClient : IGmlApplicationSchemaRegistryHttpClient
    {
        private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<GmlApplicationSchemaRegistryHttpClient> _logger;
        private readonly GmlApplicationSchemaRegistrySettings _settings;

        public GmlApplicationSchemaRegistryHttpClient(
            HttpClient client,
            IOptions<GmlApplicationSchemaRegistrySettings> options,
            ILogger<GmlApplicationSchemaRegistryHttpClient> logger)
        {
            _httpClient = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<List<ApplicationSchema>> GetGmlApplicationSchemaRegistryAsync()
        {
            if (ShouldCreateGmlApplicationSchemaRegistry())
                return await CreateGmlApplicationSchemaRegistryAsync();

            return await LoadDataFromDiskAsync();
        }

        public async Task<List<ApplicationSchema>> CreateGmlApplicationSchemaRegistryAsync()
        {
            var containedItemsResult = await FetchJsonAsync(_settings.RegisterUri);

            if (containedItemsResult == null)
                return new();

            var registerUris = containedItemsResult["containeditems"]
                .Select(token => new Uri($"{token["id"]}.json"))
                .ToList();

            var applicationSchemas = new List<ApplicationSchema>();

            foreach (var uri in registerUris)
            {
                var jObject = await FetchJsonAsync(uri);

                if (jObject == null)
                    continue;

                var applicationSchema = new ApplicationSchema
                {
                    Id = jObject["id"].ToString(),
                    Label = jObject["label"].ToString()
                };

                var version = CreateApplicationSchemaVersion(jObject);

                if (version == null)
                    continue;

                var versions = new List<ApplicationSchemaVersion> { version };

                var versionsToken = jObject["versions"];

                if (versionsToken != null && versionsToken.Any())
                {
                    foreach (var token in versionsToken)
                    {
                        var schVersion = CreateApplicationSchemaVersion(token);

                        if (schVersion != null)
                            versions.Add(schVersion);
                    }                        
                }

                applicationSchema.Versions = versions
                    .OrderByDescending(version => version.VersionName)
                    .ToList();

                applicationSchemas.Add(applicationSchema);
            }

            var ordered = applicationSchemas
                .OrderBy(schema => schema.Label)
                .ToList();
                
            await SaveDataToDiskAsync(ordered);

            return ordered;
        }

        private async Task<JObject> FetchJsonAsync(Uri uri)
        {
            try
            {
                using var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var jsonReader = new JsonTextReader(new StreamReader(stream));
                
                return await JObject.LoadAsync(jsonReader);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke hente JSON-filen '{uri}'.", uri);
                return null;
            }
        }

        private async Task SaveDataToDiskAsync(object data)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settings.CacheFilePath));

            await File.WriteAllTextAsync(_settings.CacheFilePath, JsonConvert.SerializeObject(data, _jsonSerializerSettings));
        }

        private async Task<List<ApplicationSchema>> LoadDataFromDiskAsync()
        {
            return JsonConvert.DeserializeObject<List<ApplicationSchema>>(await File.ReadAllTextAsync(_settings.CacheFilePath));
        }

        private bool ShouldCreateGmlApplicationSchemaRegistry()
        {
            return !File.Exists(_settings.CacheFilePath) || 
                DateTime.Now.Subtract(File.GetLastWriteTime(_settings.CacheFilePath)).TotalDays > _settings.CacheDurationDays;
        }

        private static ApplicationSchemaVersion CreateApplicationSchemaVersion(JToken token)
        {
            if (!Uri.TryCreate(token["documentreference"]?.ToString(), UriKind.Absolute, out var documentReference))
                return null;

            var versionNumber = token["versionNumber"].Value<int>();

            return new ApplicationSchemaVersion
            {
                VersionNumber = versionNumber,
                VersionName = token["versionName"]?.ToString() ?? versionNumber.ToString(),
                Status = GetStatus(token["status"]),
                Date = DateTime.Parse(token["dateSubmitted"].ToString()),
                DocumentReference = documentReference
            };
        }

        private static string GetStatus(JToken statusToken)
        {
            return statusToken.ToString() switch
            {
                "Gyldig" => "Gjeldende",
                "Erstattet" => "Historisk",
                "Utkast" or "Sendt inn" => "Forslag",
                _ => "",
            };
        }
    }
}
