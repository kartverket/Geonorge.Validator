using Geonorge.Validator.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.FileHelper;

namespace Geonorge.Validator.Application.HttpClients.JsonSchema
{
    public class JsonSchemaHttpClient : IJsonSchemaHttpClient
    {
        private static readonly Regex _jsonSchemaRegex = new(@"""\$schema"":\s*""(?<uri>(.*?))""", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly ILogger<JsonSchemaHttpClient> _logger;

        public JsonSchemaHttpClient(
            HttpClient client,
            ILogger<JsonSchemaHttpClient> logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        public async Task<JSchema> GetJsonSchemaAsync(List<IFormFile> jsonFiles, IFormFile schemaFile)
        {
            Stream stream;

            if (schemaFile == null)
            {
                var schemaUri = GetSchemaUriFromJsonFiles(jsonFiles);
                stream = await FetchJsonSchemaAsync(schemaUri);
            }
            else
            {
                stream = schemaFile.OpenReadStream();
            }

            using var jsonReader = new JsonTextReader(new StreamReader(stream));

            return JSchema.Load(jsonReader, new JSchemaUrlResolver());
        }

        private async Task<MemoryStream> FetchJsonSchemaAsync(string schemaUri)
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

        private static string GetSchemaUriFromJsonFiles(List<IFormFile> jsonFiles)
        {
            var schemaUris = new List<string>();

            foreach (var jsonFile in jsonFiles)
            {
                var json = ReadLines(jsonFile.OpenReadStream(), 50);
                var match = _jsonSchemaRegex.Match(json);

                if (!match.Success)
                    continue;

                schemaUris.Add(match.Groups["uri"].Value);
            }

            if (!schemaUris.Any())
                throw new InvalidJsonSchemaException("Filene i datasettet mangler applikasjonsskjema.");

            if (schemaUris.Count != jsonFiles.Count || schemaUris.Distinct().Count() != 1)
                throw new InvalidJsonSchemaException("Filene i datasettet har ulike applikasjonsskjemaer.");

            return schemaUris.First();
        }
    }
}
