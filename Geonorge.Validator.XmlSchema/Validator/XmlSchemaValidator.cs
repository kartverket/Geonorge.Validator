using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Exceptions;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static Geonorge.Validator.XmlSchema.Utils.XmlSchemaHelper;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public class XmlSchemaValidator : IXmlSchemaValidator
    {
        private readonly XmlSchemaValidatorSettings _settings;

        public XmlSchemaValidator(
            IOptions<XmlSchemaValidatorSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<XmlSchemaValidatorResult> ValidateAsync(InputData inputData, XmlSchemaData xmlSchemaData, string xmlNamespace)
        {
            try
            {
                var xmlSchemaSet = CreateXmlSchemaSet(xmlSchemaData, _settings);
                var validation = new Validation(_settings.MaxMessageCount);
                var codelistSelectors = !_settings.IgnoredNamespaces.Contains(xmlNamespace) ?
                    _settings.CodelistSelectors :
                    new();

                return await validation.Validate(inputData, xmlSchemaSet, xmlSchemaData.Streams[0], codelistSelectors);
            }
            catch (Exception exception)
            {
                throw new XmlSchemaValidationException("Kunne ikke utføre validering mot XML-skjema.", exception);
            }
        }
    }
}
