using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public interface IXmlSchemaValidator
    {
        Task<XmlSchemaValidatorResult> ValidateAsync(InputData inputData, XmlSchemaSet xmlSchemaSet, XmlSchemaData xmlSchemaData, List<string> xmlNamespaces);
    }
}
