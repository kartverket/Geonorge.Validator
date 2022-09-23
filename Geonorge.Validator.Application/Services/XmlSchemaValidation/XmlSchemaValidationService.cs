using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Rules.XmlSchema;
using Geonorge.Validator.XmlSchema.Models;
using Geonorge.Validator.XmlSchema.Validator;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XmlSchemaValidation
{
    public class XmlSchemaValidationService : IXmlSchemaValidationService
    {
        private readonly IXmlSchemaValidator _xmlSchemaValidator;
        private readonly ILogger<XmlSchemaValidationService> _logger;

        public XmlSchemaValidationService(
            IXmlSchemaValidator xmlSchemaValidator,
            ILogger<XmlSchemaValidationService> logger)
        {
            _xmlSchemaValidator = xmlSchemaValidator;
            _logger = logger;
        }

        public async Task<XmlSchemaValidationResult> ValidateAsync(
            DisposableList<InputData> inputData, XmlSchemaData xmlSchemaData, string xmlNamespace)
        {
            var xmlSchemaRule = GetXmlSchemaRule();
            var startTime = DateTime.Now;
            var codelistUris = new Dictionary<string, Uri>();

            foreach (var data in inputData)
            {
                var result = await _xmlSchemaValidator.ValidateAsync(data, xmlSchemaData, xmlNamespace);

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

                codelistUris.Append(result.CodelistUris);
            }

            xmlSchemaRule.Status = !xmlSchemaRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(xmlSchemaRule, startTime);

            return new XmlSchemaValidationResult
            {
                Rule = xmlSchemaRule,
                CodelistUris = codelistUris
            };
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
