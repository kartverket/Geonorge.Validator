using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class KodeverdiMåVæreIHenholdTilKodeliste : Rule<IGenericGmlValidationData>
    {
        public override void Create()
        {
            Id = "gml.kod.1";
            Name = "Kodeverdi må være i henhold til kodeliste";
        }

        protected override Status Validate(IGenericGmlValidationData data)
        {
            if (!data.CodeSpaces.Any() || !data.Surfaces.Any() && !data.Solids.Any())
                return Status.NOT_EXECUTED;

            data.Surfaces.Concat(data.Solids).ToList().ForEach(document => Validate(document, data.CodeSpaces));

            return HasMessages ? Status.FAILED : Status.PASSED;
        }

        private void Validate(GmlDocument document, List<CodeSpace> codeSpaces)
        {
            foreach (var codeSpace in codeSpaces)
            {
                var codeSpaceElements = GetCodeSpaceElements(document, codeSpace);

                foreach (var codeSpaceElement in codeSpaceElements)
                {
                    var codevalue = codeSpaceElement.Value;

                    if (!codeSpace.Codelist.Any(codelistValue => codelistValue.Codevalue == codevalue))
                    {
                        this.AddMessage(
                            $"Kodeverdien '{codevalue}' er ikke i henhold til kodelisten '{codeSpace.Url}'",
                            document.FileName,
                            new[] { codeSpaceElement.GetXPath() }
                        );
                    }
                }
            }
        }

        private static List<XElement> GetCodeSpaceElements(GmlDocument document, CodeSpace codeSpace)
        {
            var xPath = codeSpace.XPath;
            var elementNames = xPath.Split("//*:").Skip(1);
            var featureName = elementNames.First();
            var features = document.GetFeatures(featureName);

            if (!features.Any())
                return document.GetFeatures().GetElements(xPath).ToList();

            var subXPath = $"//*:{string.Join("//*:", elementNames.Skip(1))}";

            return features.GetElements(subXPath).ToList();
        }
    }
}
