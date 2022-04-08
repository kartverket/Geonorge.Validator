using Geonorge.XsdValidator.Translator;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Geonorge.XsdValidator.Validator
{
    internal class Validation
    {
        private readonly int _maxMessageCount;
        private readonly List<string> _messages;

        public Validation(int maxMessageCount = 1000)
        {
            _maxMessageCount = maxMessageCount;
            _messages = new List<string>();
        }

        public List<string> Validate(Stream xmlStream, XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = GetXmlReaderSettings(xmlSchemaSet);

            Validate(xmlStream, xmlReaderSettings);

            return _messages;
        }

        private void Validate(Stream xmlStream, XmlReaderSettings xmlReaderSettings)
        {
            using var reader = XmlReader.Create(xmlStream, xmlReaderSettings);

            try
            {
                while (reader.Read())
                {
                    if (_messages.Count >= _maxMessageCount)
                        break;
                }
            }
            catch (XmlException exception)
            {
                _messages.Add(MessageTranslator.TranslateError(exception.Message));
            }
        }

        private XmlReaderSettings GetXmlReaderSettings(XmlSchemaSet xmlSchemaSet)
        {
            var xmlReaderSettings = new XmlReaderSettings { ValidationType = ValidationType.Schema };

            xmlReaderSettings.Schemas.Add(xmlSchemaSet);            
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xmlReaderSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            xmlReaderSettings.ValidationFlags &= ~XmlSchemaValidationFlags.ProcessIdentityConstraints;
            xmlReaderSettings.ValidationEventHandler += ValidationCallBack;

            return xmlReaderSettings;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            var prefix = $"Linje {args.Exception.LineNumber}, posisjon {args.Exception.LinePosition}: ";

            switch (args.Severity)
            {
                case XmlSeverityType.Error:
                    prefix += MessageTranslator.TranslateError(args.Message);
                    break;
                case XmlSeverityType.Warning:
                    prefix += MessageTranslator.TranslateWarning(args.Message);
                    break;
            }

            _messages.Add(prefix);
        }
    }
}
