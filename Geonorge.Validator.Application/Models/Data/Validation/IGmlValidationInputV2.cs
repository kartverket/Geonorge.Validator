using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Models.Data.Codelist;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public interface IGmlValidationInputV2 : IGmlValidationInputV1
    {
        List<GmlCodeSpace> CodeSpaces { get; }
        XLinkResolver XLinkResolver { get; }
    }
}
