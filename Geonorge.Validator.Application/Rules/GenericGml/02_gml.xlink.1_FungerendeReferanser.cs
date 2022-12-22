using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Extensions.Gml.Constants;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.XmlSchema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static Geonorge.Validator.Common.Helpers.XmlHelper;

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
                await ValidateAsync(documents, document, input.XLinkResolver);
        }

        private async Task ValidateAsync(IEnumerable<GmlDocument> documents, GmlDocument document, XLinkResolver xLinkResolver)
        {
            var qualifiedName = new XmlQualifiedName("ReferenceType", Namespace.GmlNs.NamespaceName);

            var codelistElementNames = xLinkResolver.XmlSchemaElements
                .Where(element => element.SchemaTypeName.Equals(qualifiedName))
                .GroupBy(element => element.QualifiedName)
                .Select(grouping =>
                {
                    XNamespace ns = grouping.Key.Namespace;
                    XName name = ns + grouping.Key.Name;
                    
                    return name;
                })
                .ToList();

            XName xLinkName = Namespace.XLinkNs + "href";

            var xLinkElements = document.Document.Root.Descendants()
                .Where(element => element.Attributes().Any(attr => attr.Name == xLinkName))
                .ToList();

            foreach (var element in xLinkElements)
            {
                if (codelistElementNames.Contains(element.Name))
                {
                    var uriString = element.Attribute(Namespace.XLinkNs + "href").Value;

                    if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                    {
                        continue;
                    }

                    var codelist = await xLinkResolver.ResolveCodelist(uri);
                }
                else
                {
                    var xLink = element.Attribute(Namespace.XLinkNs + "href")?.Value.Split("#");

                    if (xLink.Length != 2)
                    {
                        this.AddMessage(
                            Translate("Message1", GmlHelper.GetFeatureType(element), element.GetName(), xLink[0]),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );

                        return;
                    }

                    var fileName = string.IsNullOrWhiteSpace(xLink[0]) ? document.FileName : xLink[0];
                    var gmlId = xLink[1];
                    var refElement = GmlHelper.GetElementByGmlId(documents, gmlId, fileName);

                    if (refElement == null)
                    {
                        this.AddMessage(
                            Translate("Message2", GmlHelper.GetNameAndId(GmlHelper.GetFeatureElement(element)), element.GetName(), gmlId),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );

                        return;
                    }

                    var qualName = new XmlQualifiedName(element.Name.LocalName, element.Name.NamespaceName);
                    var xmlSchemaElement = xLinkResolver.XmlSchemaElements.Where(el => el.QualifiedName == qualName).ToList();
                }
            }

            /*if (!xLinkResolver.XLinkElements.TryGetValue(document.FileName, out var xLinkElements3))
                return;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            await Parallel.ForEachAsync(xLinkElements3, parallelOptions, async (xLinkElement, token) =>
            {
                if (xLinkElement.Type == XLinkType.Object)
                {
                    var element = GetElementAtLine(document.Document, xLinkElement.LineNumber, xLinkElement.LinePosition);

                    if (element == null)
                        return;

                    var xLink = element.Attribute(Namespace.XLinkNs + "href")?.Value.Split("#");

                    if (xLink.Length != 2)
                    {
                        this.AddMessage(
                            Translate("Message1", GmlHelper.GetFeatureType(element), element.GetName(), xLink[0]),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );

                        return;
                    }

                    var fileName = string.IsNullOrWhiteSpace(xLink[0]) ? document.FileName : xLink[0];
                    var gmlId = xLink[1];
                    var refElement = GmlHelper.GetElementByGmlId(documents, gmlId, fileName);

                    if (refElement == null)
                    {
                        this.AddMessage(
                            Translate("Message2", GmlHelper.GetNameAndId(GmlHelper.GetFeatureElement(element)), element.GetName(), gmlId),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );

                        return;
                    }

                    var validationResult = xLinkResolver.Validate(xLinkElement, element, refElement);

                    if (validationResult != default)
                    {
                        this.AddMessage(
                            Translate("Message3", GmlHelper.GetNameAndId(GmlHelper.GetFeatureElement(element)), validationResult.RefElement, gmlId, validationResult.ValidElements),
                            document.FileName,
                            new[] { element.GetXPath() },
                            new[] { GmlHelper.GetFeatureGmlId(element) }
                        );
                    }
                }
                else if (xLinkElement.Type == XLinkType.Codelist)
                {
                    var element = GetElementAtLine(document.Document, xLinkElement.LineNumber, xLinkElement.LinePosition);

                    if (element == null)
                        return;

                    var uri = element.Attribute(Namespace.XLinkNs + "href").Value;
                    var resolverResult = await xLinkResolver.ResolveCodelist(uri);

                    if (resolverResult.ResolverStatus == CodelistResolverStatus.ValueFound)
                        return;

                    this.AddMessage(
                        GetCodelistErrorMessage(resolverResult),
                        document.FileName,
                        new[] { element.GetXPath() },
                        new[] { GmlHelper.GetFeatureGmlId(element) }
                    );
                }
            });*/
        }

        private string GetCodelistErrorMessage(CodelistResolverResult resolverResult)
        {
            return resolverResult.ResolverStatus switch
            {
                CodelistResolverStatus.MissingUriFragment => Translate("Message4", resolverResult.Url),
                CodelistResolverStatus.ValueNotFound => Translate("Message5", resolverResult.Fragment, resolverResult.Url),
                CodelistResolverStatus.CodelistNotFound => Translate("Message6", resolverResult.Url, (int)resolverResult.HttpStatusCode),
                CodelistResolverStatus.CodelistUnavailable => Translate("Message7", resolverResult.Url, (int)resolverResult.HttpStatusCode),
                CodelistResolverStatus.InvalidCodelist => Translate("Message8", resolverResult.Url),
                _ => null,
            };
        }
    }
}
