using Geonorge.Validator.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public class XsdHttpClient : IXsdHttpClient
    {
        private static readonly Regex _schemaLocationRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        private readonly ILogger<XsdHttpClient> _logger;
        public HttpClient Client { get; }

        public XsdHttpClient(
            HttpClient client,
            ILogger<XsdHttpClient> logger)
        {
            Client = client;
            _logger = logger;
        }

        public async Task<Stream> GetXsdFromXmlFiles(List<IFormFile> xmlFiles)
        {
            var schemaUri = GetSchemaUriFromXmlFiles(xmlFiles);

            return await FetchXsd(schemaUri);
        }

        private async Task<Stream> FetchXsd(string schemaUri)
        {
            try
            {
                using var response = await Client.GetAsync(schemaUri);
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

        private static string GetSchemaUriFromXmlFiles(List<IFormFile> xmlFiles)
        {
            var schemaUris = xmlFiles
                .Select(xmlFile =>
                {
                    var xmlString = ReadLines(xmlFile.OpenReadStream(), 10);
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
    }
}
