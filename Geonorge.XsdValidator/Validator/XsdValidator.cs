using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Exceptions;
using Geonorge.XsdValidator.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using static Geonorge.XsdValidator.Utils.XsdHelper;

namespace Geonorge.XsdValidator.Validator
{
    public class XsdValidator : IXsdValidator
    {
        private readonly XsdValidatorSettings _settings;

        public XsdValidator(
            IOptions<XsdValidatorSettings> options)
        {
            _settings = options.Value;
        }

        public XsdValidatorResult Validate(Stream xmlStream, XsdData xsdData)
        {
            try
            {
                var xmlSchemaSet = CreateXmlSchemaSet(xsdData, _settings);

                return new Validation(_settings.MaxMessageCount).Validate(xmlStream, xmlSchemaSet, xsdData.Streams[0], _settings.CodelistSelectors);
            }
            catch (Exception exception)
            {
                throw new XmlSchemaValidationException("Kunne ikke utføre validering mot XML-skjema.", exception);
            }
        }
    }
}
