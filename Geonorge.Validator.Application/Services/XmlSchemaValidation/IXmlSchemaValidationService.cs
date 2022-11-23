using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.XmlSchema.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XmlSchemaValidation
{
    public interface IXmlSchemaValidationService
    {
        Task<XmlSchemaValidationResult> ValidateAsync(DisposableList<InputData> inputData, XmlSchemaData xmlSchemaData, List<string> xmlNamespace);
    }
}
