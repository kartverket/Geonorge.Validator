using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Exceptions;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static Geonorge.Validator.XmlSchema.Utils.XsdHelper;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public class XsdValidator : IXsdValidator
    {
        private readonly XsdValidatorSettings _settings;

        public XsdValidator(
            IOptions<XsdValidatorSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<XsdValidatorResult> ValidateAsync(InputData inputData, XmlSchemaData xsdData)
        {
            try
            {
                var xmlSchemaSet = CreateXmlSchemaSet(xsdData, _settings);
                var validation = new Validation(_settings.MaxMessageCount);

                return await validation.Validate(inputData, xmlSchemaSet, xsdData.Streams[0], _settings.CodelistSelectors);
            }
            catch (Exception exception)
            {
                throw new XmlSchemaValidationException("Kunne ikke utføre validering mot XML-skjema.", exception);
            }
        }
    }
}
