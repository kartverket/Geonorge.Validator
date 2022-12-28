using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Extensions.Gml.Constants;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class FungerendeReferanser : Rule<IGmlValidationInputV2>
    {
        public override void Create()
        {
            Id = "gml.xlink.1";
        }

        protected override async Task ValidateAsync(IGmlValidationInputV2 input)
        {
            if (!input.Surfaces.Any() && !input.Solids.Any())
                SkipRule();

            var documents = input.Surfaces.Concat(input.Solids);

            foreach (var document in documents)
                await ValidateAsync(documents, document, input.XLinkValidator);
        }

        private async Task ValidateAsync(IEnumerable<GmlDocument> documents, GmlDocument document, XLinkValidator xLinkValidator)
        {
            XName xLinkName = Namespace.XLinkNs + "href";

            var xLinkElements = document.Document.Root.Descendants()
                .Where(element => element.Attributes().Any(attr => attr.Name == xLinkName));

            if (!xLinkElements.Any())
                SkipRule();

            var codelistElementNames = GetCodelistElementNames(xLinkValidator.XmlSchemaElements);

            var codelistXLinkElements = xLinkElements
                .Where(element => codelistElementNames.Contains(element.Name))
                .ToList();

            foreach (var element in codelistXLinkElements)
            {
                var uriString = element.Attribute(Namespace.XLinkNs + "href")?.Value;

                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                {
                    this.AddMessage(
                        Translate("Message1", GmlHelper.GetNameAndId(element)),
                        document.FileName,
                        new[] { element.GetXPath() },
                        new[] { GmlHelper.GetFeatureGmlId(element) }
                    );
                }
                else if (string.IsNullOrWhiteSpace(uri.Fragment))
                {
                    this.AddMessage(
                        Translate("Message2", uri.AbsoluteUri),
                        document.FileName,
                        new[] { element.GetXPath() },
                        new[] { GmlHelper.GetFeatureGmlId(element) }
                    );
                }
                else
                {
                    var urlAndFragment = uri.AbsoluteUri.Split("#");
                    var url = new Uri(urlAndFragment[0]);
                    var codelist = await xLinkValidator.FetchCodelist(url);

                    if (codelist.Status == CodelistStatus.CodelistNotFound)
                    {
                        this.AddMessage(
                            Translate("Message3", uri.AbsoluteUri, (int)codelist.HttpStatusCode),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (codelist.Status == CodelistStatus.CodelistUnavailable)
                    {
                        this.AddMessage(
                            Translate("Message4", uri.AbsoluteUri, (int)codelist.HttpStatusCode),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (codelist.Status == CodelistStatus.InvalidCodelist)
                    {
                        this.AddMessage(
                            Translate("Message5", uri.AbsoluteUri),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else if (!codelist.Items.Any(item => item.Value == urlAndFragment[1]))
                    {
                        this.AddMessage(
                            Translate("Message6", urlAndFragment[1], uri.AbsoluteUri),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                }
            }

            var objectXLinkElements = xLinkElements
                .Where(element => !codelistElementNames.Contains(element.Name))
                .ToList();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.ForEach(objectXLinkElements, parallelOptions, element =>
            {
                var xLink = element.Attribute(Namespace.XLinkNs + "href")?.Value.Split("#");

                if (xLink.Length != 2)
                {
                    this.AddMessage(
                        Translate("Message7", GmlHelper.GetFeatureType(element), element.GetName(), xLink[0]),
                        document.FileName,
                        new[] { element.GetXPath() },
                        new[] { GmlHelper.GetFeatureGmlId(element) }
                    );
                }
                else
                {
                    var fileName = string.IsNullOrWhiteSpace(xLink[0]) ? document.FileName : xLink[0];
                    var gmlId = xLink[1];
                    var refElement = GmlHelper.GetElementByGmlId(documents, gmlId, fileName);

                    if (refElement == null)
                    {
                        this.AddMessage(
                            Translate("Message8", GmlHelper.GetNameAndId(GmlHelper.GetFeatureElement(element)), element.GetName(), gmlId),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                    else
                    {
                        var qualifiedName = new XmlQualifiedName(element.Name.LocalName, element.Name.NamespaceName);
                        var xmlSchemaElement = xLinkValidator.XmlSchemaElements.SingleOrDefault(element => element.QualifiedName == qualifiedName);

                        if (xmlSchemaElement == null)
                            return;

                        var validationResult = xLinkValidator.Validate(element, refElement, xmlSchemaElement);

                        if (validationResult != default)
                        {
                            this.AddMessage(
                                Translate("Message9", GmlHelper.GetNameAndId(GmlHelper.GetFeatureElement(element)), validationResult.RefElement, gmlId, validationResult.ValidElements),
                                document.FileName,
                                new[] { element.GetXPath() },
                                new[] { GmlHelper.GetFeatureGmlId(element) }
                            );
                        }
                    }
                }
            });
        }

        private static List<XName> GetCodelistElementNames(HashSet<XmlSchemaElement> xmlSchemaElements)
        {
            var qualifiedName = new XmlQualifiedName("ReferenceType", Namespace.GmlNs.NamespaceName);

            return xmlSchemaElements
                .Where(element => element.SchemaTypeName.Equals(qualifiedName))
                .GroupBy(element => element.QualifiedName)
                .Select(grouping =>
                {
                    XNamespace ns = grouping.Key.Namespace;
                    XName name = ns + grouping.Key.Name;

                    return name;
                })
                .ToList();
        }
    }
}
