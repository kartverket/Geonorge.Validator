using DiBK.RuleValidator.Rules.Gml;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public interface IGmlValidationInputV2 : IGmlValidationInputV1
    {
        XLinkValidator XLinkValidator { get; }
    }
}
