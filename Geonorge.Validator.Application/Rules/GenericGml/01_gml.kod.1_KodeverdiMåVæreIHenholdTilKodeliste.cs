using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static DiBK.RuleValidator.Extensions.Gml.Constants.Namespace;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class KodeverdiMåVæreIHenholdTilEksternKodeliste : Rule<IGmlValidationInputV2>
    {
        public override void Create()
        {
            Id = "gml.kod.1";
        }

        protected override async Task ValidateAsync(IGmlValidationInputV2 data)
        {
            if (!data.Surfaces.Any() && !data.Solids.Any())
                SkipRule();

            var codelists = await GetCodelists(data.XLinkValidator.XmlSchemaElements, data.XLinkValidator.FetchCodelist);

            if (!codelists.Any())
                SkipRule();

            var documents = data.Surfaces.Concat(data.Solids);

            foreach (var document in documents)
                Validate(document, codelists);
        }

        private void Validate(GmlDocument document, Dictionary<XName, Codelist> codelists)
        {
            var codelistElements = document.Document.Descendants()
                .Where(element => codelists.ContainsKey(element.Name))
                .ToList();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.ForEach(codelistElements, parallelOptions, element =>
            {
                var code = element.Value;

                if (codelists.TryGetValue(element.Name, out var codelist))
                {
                    if (codelist.Status == CodelistStatus.CodelistNotFound)
                    {
                        this.AddMessage(
                            Translate("Message1", codelist.Uri.AbsoluteUri, (int)codelist.HttpStatusCode),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (codelist.Status == CodelistStatus.CodelistUnavailable)
                    {
                        this.AddMessage(
                            Translate("Message2", codelist.Uri.AbsoluteUri, (int)codelist.HttpStatusCode),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (codelist.Status == CodelistStatus.InvalidCodelist)
                    {
                        this.AddMessage(
                            Translate("Message3", codelist.Uri.AbsoluteUri),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (!codelist.Items.Any(codelistValue => codelistValue.Value == code))
                    {
                        this.AddMessage(
                            Translate("Message4", code, codelist.Uri.AbsoluteUri),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                }
            });
        }

        private static async Task<Dictionary<XName, Codelist>> GetCodelists(
            HashSet<XmlSchemaElement> xmlSchemaElements, Func<Uri, Task<Codelist>> fetchCodelist)
        {
            var codeElements = xmlSchemaElements
                .Where(element => _codelistSelectors.ContainsKey(element.SchemaTypeName))
                .GroupBy(element => element.QualifiedName)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            var codeListDict = new Dictionary<XName, Codelist>();

            foreach (var (qualifiedName, elements) in codeElements)
            {
                XNamespace ns = qualifiedName.Namespace;
                XName name = ns + qualifiedName.Name;

                var uris = new HashSet<Uri>();

                foreach (var element in elements)
                {
                    if (_codelistSelectors.TryGetValue(element.SchemaTypeName, out var resolveUri))
                        uris.Add(resolveUri(element));
                }

                var uri = uris
                    .Where(uri => uri != null)
                    .FirstOrDefault();

                if (uri != null)
                {
                    var codelist = await fetchCodelist(uri);
                    codeListDict.Add(name, codelist);
                }
            }

            return codeListDict;
        }

        private static readonly Dictionary<XmlQualifiedName, Func<XmlSchemaElement, Uri>> _codelistSelectors = new()
        {
            {
                new XmlQualifiedName("CodeType", GmlNs.NamespaceName),
                element =>
                {
                    var uriString = element.Annotation?
                        .Items
                        .OfType<XmlSchemaAppInfo>()
                        .SingleOrDefault()?
                        .Markup
                        .SingleOrDefault(node => node.LocalName == "defaultCodeSpace")?
                        .InnerText;

                    if (uriString == null)
                        return null;

                    return Uri.TryCreate(uriString, UriKind.Absolute, out var uri) ? uri : null;
                }
            }
        };
    }
}
