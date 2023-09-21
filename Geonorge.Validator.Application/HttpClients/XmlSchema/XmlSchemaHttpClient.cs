using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Common.Exceptions;
using Geonorge.Validator.Common.Helpers;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.XmlSchema
{
    public class XmlSchemaHttpClient : IXmlSchemaHttpClient
    {
        private static readonly Regex _schemaLocationRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        private readonly HttpClient _httpClient;
        private readonly XmlSchemaValidatorSettings _settings;
        private readonly ILogger<XmlSchemaHttpClient> _logger;

        public XmlSchemaHttpClient(
            HttpClient client,
            IOptions<XmlSchemaValidatorSettings> options,
            ILogger<XmlSchemaHttpClient> logger)
        {
            _httpClient = client;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<XmlSchemaData> GetXmlSchemaFromInputDataAsync(DisposableList<InputData> inputData)
        {
            var xsdData = new XmlSchemaData
            {
                SchemaUris = GetSchemaUriFromXmlFiles(inputData)
            };

            foreach (var schemaUri in xsdData.SchemaUris)
                xsdData.Streams.Add(await FetchXmlSchemaAsync(schemaUri));

            return xsdData;
        }

        public async Task<MemoryStream> FetchXmlSchemaAsync(Uri uri)
        {
            if (uri.IsFile)
                return await LoadXmlSchemaFromDiskAsync(uri);

            try
            {
                using var response = await _httpClient.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                return memoryStream;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.", uri);
                throw new InvalidXmlSchemaException($"Kunne ikke hente applikasjonsskjemaet '{uri}'.");
            }
        }

        private static async Task<MemoryStream> LoadXmlSchemaFromDiskAsync(Uri uri)
        {
            if (!File.Exists(uri.AbsolutePath))
                throw new InvalidXmlSchemaException($"Kunne ikke hente applikasjonsskjemaet '{uri}'. Filen finnes ikke.");

            using var stream = File.OpenRead(uri.AbsolutePath);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

        private static List<Uri> GetSchemaUriFromXmlFiles(DisposableList<InputData> inputData)
        {
            var schemaUrisList = inputData
                .Select(data =>
                {
                    var xmlString = FileHelper.ReadLines(data.Stream, 50);
                    var match = _schemaLocationRegex.Match(xmlString);

                    if (!match.Success)
                        return null;

                    var values = match.Groups["schema_loc"].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var uris = new List<string>();

                    for (var i = 1; i < values.Length; i += 2)
                        uris.Add(values[i]);

                    return uris;
                })
                .ToList();

            if (!schemaUrisList.Any())
                throw new InvalidXmlSchemaException("Filene i datasettet mangler applikasjonsskjema.");

            if (schemaUrisList.Count != inputData.Count || !XmlFilesHaveSameSchemas(schemaUrisList))
                throw new InvalidXmlSchemaException("Filene i datasettet har ulike applikasjonsskjemaer.");

            return schemaUrisList
                .First()
                .Select(uriString =>
                {
                    if (!Uri.TryCreate(uriString, new UriCreationOptions(), out var uri))
                        throw new InvalidXmlSchemaException($"Applikasjonsskjemaet '{uriString}' er en ugyldig URI.");

                    return uri;
                })
                .ToList();
        }

        private static bool XmlFilesHaveSameSchemas(List<List<string>> schemaUrisList)
        {
            for (var i = 0; i < schemaUrisList.Count - 1; i++)
            {
                var schemaUris = schemaUrisList[i];

                for (int j = i + 1; j < schemaUrisList.Count; j++)
                {
                    var otherSchemaUris = schemaUrisList[j];

                    if (!schemaUris.SequenceEqual(otherSchemaUris))
                        return false;
                }
            }

            return true;
        }
    }
}
