using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Rules.XmlSchema;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Geonorge.Validator.XmlSchema.Validator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.Validator.XmlSchema.Utils.XmlSchemaHelper;
using Xsd = System.Xml.Schema.XmlSchema;

namespace Geonorge.Validator.Application.Services.XmlSchemaValidation
{
    public class XmlSchemaValidationService : IXmlSchemaValidationService
    {
        private readonly IXmlSchemaValidator _xmlSchemaValidator;
        private readonly XmlSchemaValidatorSettings _settings;
        private readonly ILogger<XmlSchemaValidationService> _logger;

        public XmlSchemaValidationService(
            IXmlSchemaValidator xmlSchemaValidator,
            IOptions<XmlSchemaValidatorSettings> options,
            ILogger<XmlSchemaValidationService> logger)
        {
            _xmlSchemaValidator = xmlSchemaValidator;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<XmlSchemaValidationResult> ValidateAsync(
            DisposableList<InputData> inputData, XmlSchemaData xmlSchemaData, List<string> xmlNamespaces)
        {            
            var xmlSchemaRule = GetXmlSchemaRule();
            var xmlSchemaSet = CreateXmlSchemaSet(xmlSchemaData, _settings);
            var xmlSchemaElements = new HashSet<XmlSchemaElement>();
            var startTime = DateTime.Now;

            foreach (var data in inputData)
            {
                var result = await _xmlSchemaValidator.ValidateAsync(data, xmlSchemaSet);

                data.IsValid = !result.Messages.Any();
                data.Stream.Position = 0;

                result.Messages
                    .Select(message =>
                    {
                        var properties = new Dictionary<string, object>
                        {
                            { "LineNumber", message.LineNumber },
                            { "LinePosition", message.LinePosition },
                            { "FileName", message.FileName }
                        };

                        if (message.XPath != null)
                            properties.Add("XPaths", new[] { message.XPath });

                        return new RuleMessage
                        {
                            Message = message.Message,
                            Properties = properties
                        };
                    })
                    .ToList()
                    .ForEach(xmlSchemaRule.AddMessage);

                xmlSchemaElements.UnionWith(result.SchemaElements);
            }

            xmlSchemaRule.Status = !xmlSchemaRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(xmlSchemaRule, startTime);

            return new XmlSchemaValidationResult(xmlSchemaRule, xmlSchemaElements, xmlSchemaSet);
        }

        private void LogInformation(XmlSchemaRule xmlSchemaRule, DateTime startTime)
        {
            _logger.LogInformation("{@Rule}", new
            {
                xmlSchemaRule.Id,
                xmlSchemaRule.Name,
                FullName = xmlSchemaRule.ToString(),
                xmlSchemaRule.Status,
                TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds,
                MessageCount = xmlSchemaRule.Messages.Count
            });
        }

        private static XmlSchemaRule GetXmlSchemaRule()
        {
            var rule = Activator.CreateInstance(typeof(Skjemavalidering)) as XmlSchemaRule;
            rule.Create();

            return rule;
        }
    }
}
