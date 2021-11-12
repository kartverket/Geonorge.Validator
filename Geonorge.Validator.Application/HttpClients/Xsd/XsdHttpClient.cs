using Geonorge.Validator.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public class XsdHttpClient : IXsdHttpClient
    {
        private static readonly Regex _xmlNamespaceRegex = new(@"xsi:schemaLocation=""(?<schema_loc>(.*?))""", RegexOptions.Compiled);

        private readonly ILogger<XsdHttpClient> _logger;
        public HttpClient Client { get; }

        public XsdHttpClient(
            HttpClient client,
            ILogger<XsdHttpClient> logger)
        {
            Client = client;
            _logger = logger;
        }

        public async Task<(string XmlNamespace, string XsdVersion)> GetXmlNamespaceAndXsdVersion(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            if (xsdFile != null)
                return await GetTargetNamespaceAndVersionFromXsd(xsdFile.OpenReadStream(), xsdFile.FileName);

            return await GetTargetNamespaceAndVersionFromXmlFiles(xmlFiles);
        }

        private async Task<(string TargetNamespace, string Version)> GetTargetNamespaceAndVersionFromXsd(Stream stream, string fileName)
        {
            try
            {
                var document = await XDocument.LoadAsync(stream, LoadOptions.None, new CancellationToken());

                return (
                    document.Root.XPath2SelectOne<XAttribute>("@targetNamespace")?.Value,
                    document.Root.XPath2SelectOne<XAttribute>("@version")?.Value
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ugyldig applikasjonsskjema: '{fileName}'.", fileName);
                throw new InvalidXsdException($"Ugyldig applikasjonsskjema: '{fileName}'.");
            }
        }

        private async Task<(string TargetNamespace, string Version)> GetTargetNamespaceAndVersionFromXsd(string schemaUri)
        {
            try
            {
                using var response = await Client.GetAsync(schemaUri);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync();

                return await GetTargetNamespaceAndVersionFromXsd(stream, schemaUri);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.", schemaUri);
                throw new InvalidXsdException($"Kunne ikke hente applikasjonsskjemaet '{schemaUri}'.");
            }
        }

        private async Task<(string TargetNamespace, string Version)> GetTargetNamespaceAndVersionFromXmlFiles(List<IFormFile> xmlFiles)
        {
            var schemaUris = xmlFiles
                .Select(xmlFile =>
                {
                    using var streamReader = new StreamReader(xmlFile.OpenReadStream());
                    var xmlString = streamReader.ReadToEnd();
                    var match = _xmlNamespaceRegex.Match(xmlString);

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

            return await GetTargetNamespaceAndVersionFromXsd(schemaUri);
        }
    }
}
