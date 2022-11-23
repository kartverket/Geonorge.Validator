using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class KodeverdiMåVæreIHenholdTilEksternKodeliste : Rule<IGmlValidationInputV2>
    {
        public override void Create()
        {
            Id = "gml.kod.1";
        }

        protected override void Validate(IGmlValidationInputV2 data)
        {
            if (!data.CodeSpaces.Any() || !data.Surfaces.Any() && !data.Solids.Any())
                SkipRule();

            data.Surfaces.Concat(data.Solids).ToList().ForEach(document => Validate(document, data.CodeSpaces));
        }

        private void Validate(GmlDocument document, List<GmlCodeSpace> gmlCodeSpaces)
        {
            foreach (var gmlCodeSpace in gmlCodeSpaces)
            {
                var featureElements = document.GetFeatureElements(gmlCodeSpace.FeatureMemberName);

                Parallel.ForEach(featureElements, element =>
                {
                    foreach (var codeSpace in gmlCodeSpace.CodeSpaces)
                    {
                        var codeElement = element.GetElement(codeSpace.XPath);

                        if (codeElement == null)
                            continue;

                        var code = codeElement.Value;

                        if (!codeSpace.Codelist.Any(codelistValue => codelistValue.Value == code))
                        {
                            this.AddMessage(
                                Translate("Message", code, codeSpace.Url),
                                document.FileName,
                                new[] { codeElement.GetXPath() },
                                new[] { element.GetAttribute("gml:id") }
                            );
                        }
                    }
                });
            }
        }
    }
}
