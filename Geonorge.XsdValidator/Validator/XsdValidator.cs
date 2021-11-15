using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Validator
{
    public class XsdValidator : IXsdValidator
    {
        private readonly XsdValidatorSettings _options;
        private readonly ILogger<XsdValidator> _logger;

        public XsdValidator(
            IOptions<XsdValidatorSettings> options,
            ILogger<XsdValidator> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public List<string> Validate(Stream xmlStream, Stream xsdStream)
        {
            var xmlSchemaSet = CreateXmlSchemaSet(xsdStream);

            return new ValidatorRun(_options).Validate(xmlStream, xmlSchemaSet);
        }

        private XmlSchemaSet CreateXmlSchemaSet(Stream xsdStream)
        {
            if (xsdStream == null)
                return null;

            var xmlSchemaSet = new XmlSchemaSet { XmlResolver = new XmlFileCacheResolver(_options) };
            var xmlSchema = XmlSchema.Read(xsdStream, null);
            xsdStream.Seek(0, SeekOrigin.Begin);

            xmlSchemaSet.Add(xmlSchema);

            return CompileSchemaSet(xmlSchemaSet);
        }

        private XmlSchemaSet CompileSchemaSet(XmlSchemaSet xmlSchemaSet)
        {
            try
            {
                xmlSchemaSet.Compile();
                return xmlSchemaSet;
            }
            catch (XmlSchemaException exception)
            {
                _logger.LogError(exception, "Could not compile XmlSchemaSet!");
                throw;
            }
        }
    }
}
