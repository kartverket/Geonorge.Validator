using DiBK.RuleValidator.Extensions.Gml.Constants;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Geonorge.Validator.Common.Helpers.XmlHelper;

namespace Geonorge.Validator.Application.HttpClients.CodelistResolver
{
    public class CodelistResolverHttpClient : ICodelistResolverHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CodelistResolverHttpClient> _logger;

        public CodelistResolverHttpClient(
            HttpClient httpClient,
            ILogger<CodelistResolverHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public List<CodelistDocument> CodelistDocuments { get; } = new();

        public async Task<CodelistResolverResult> ValidateCodelistUriAsync(string uriString)
        {
            if (!TryCreateUrlAndFragment(uriString, out var urlAndFragment))
                return new CodelistResolverResult(CodelistResolverStatus.MissingUriFragment, uriString, null);

            (Uri url, string fragment) = urlAndFragment;
            var document = await GetCodelistDocumentAsync(url);

            if (document.Document != null)
            {
                var definitionElement = document.Document.Root.Descendants(Namespace.GmlNs + "Definition")
                    .SingleOrDefault(element => element.Attribute(Namespace.GmlNs + "id")?.Value == fragment);

                if (definitionElement != null)
                    return new CodelistResolverResult(CodelistResolverStatus.ValueFound, document.StatusCode, url.AbsoluteUri, fragment);

                return new CodelistResolverResult(CodelistResolverStatus.ValueNotFound, document.StatusCode, url.AbsoluteUri, fragment);
            }

            if (document.StatusCode == HttpStatusCode.OK)
                return new CodelistResolverResult(CodelistResolverStatus.InvalidCodelist, document.StatusCode, url.AbsoluteUri, fragment);

            if (document.StatusCode == HttpStatusCode.NotFound)
                return new CodelistResolverResult(CodelistResolverStatus.CodelistNotFound, document.StatusCode, url.AbsoluteUri, fragment);

            return new CodelistResolverResult(CodelistResolverStatus.CodelistUnavailable, document.StatusCode, url.AbsoluteUri, fragment);
        }

        private async Task<CodelistDocument> GetCodelistDocumentAsync(Uri uri)
        {
            var codelistDocument = CodelistDocuments
                .SingleOrDefault(document => document.Uri.Equals(uri));

            if (codelistDocument != null)
                return codelistDocument;

            return await DownloadAsync(uri);
        }

        private async Task<CodelistDocument> DownloadAsync(Uri uri)
        {
            using var response = await _httpClient.GetAsync(uri);
            CodelistDocument codelistDocument = null;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                codelistDocument = new CodelistDocument(uri, null, response.StatusCode);
                CodelistDocuments.Add(codelistDocument);
                
                return codelistDocument;
            }

            using var content = await response.Content.ReadAsStreamAsync();
            XDocument document = null;

            try
            {
                document = await LoadXDocumentAsync(content);                
            }
            catch
            {
            }

            codelistDocument = new CodelistDocument(uri, document, response.StatusCode);
            CodelistDocuments.Add(codelistDocument);

            return codelistDocument;
        }

        private static bool TryCreateUrlAndFragment(string uri, out (Uri Uri, string Fragment) urlAndFragment)
        {
            urlAndFragment = default;
            var splitted = uri.Split("#");

            if (splitted.Length != 2 || !Uri.TryCreate(splitted[0], UriKind.Absolute, out var resultUri) && (resultUri.Scheme == Uri.UriSchemeHttp || resultUri.Scheme == Uri.UriSchemeHttps))
                return false;

            urlAndFragment = (resultUri, splitted[1]);
            return true;
        }
    }
}
