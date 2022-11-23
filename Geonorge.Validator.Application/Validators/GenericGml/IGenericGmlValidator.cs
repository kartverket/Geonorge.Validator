using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public interface IGenericGmlValidator
    {
        Task<List<Rule>> Validate(
            DisposableList<InputData> inputData, Dictionary<string, Uri> codelistUris, Dictionary<string, List<XLinkElement>> xLinkElements, XmlSchemaSet xmlSchemaSet, List<string> skipRules);
    }
}
