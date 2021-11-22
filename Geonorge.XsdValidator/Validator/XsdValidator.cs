using Geonorge.XsdValidator.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
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

        public List<string> Validate(Stream xmlStream, Stream xsdStream)
        {
            var xmlSchemaSet = CreateXmlSchemaSet(xsdStream, _settings);

            return new ValidatorRun().Validate(xmlStream, xmlSchemaSet);
        }
    }
}
