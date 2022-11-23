using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Exceptions;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using XmlSchemaValidationException = Geonorge.Validator.XmlSchema.Exceptions.XmlSchemaValidationException;

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

        public async Task<XmlSchemaValidatorResult> ValidateAsync(
            InputData inputData, XmlSchemaSet xmlSchemaSet, XmlSchemaData xmlSchemaData, List<string> xmlNamespaces)
        {
            try
            {
                var validation = new Validation(_settings.MaxMessageCount);
                var codelistSelectors = !_settings.IgnoredNamespaces.Any(ignoredNs => xmlNamespaces.Contains(ignoredNs)) ?
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
