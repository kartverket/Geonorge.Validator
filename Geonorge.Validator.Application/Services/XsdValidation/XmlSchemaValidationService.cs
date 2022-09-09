using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Rules.XmlSchema;
using Geonorge.XsdValidator.Models;
using Geonorge.XsdValidator.Validator;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public class XmlSchemaValidationService : IXmlSchemaValidationService
    {
        private readonly IXsdValidator _xsdValidator;
        private readonly ILogger<XmlSchemaValidationService> _logger;

        public XmlSchemaValidationService(
            IXsdValidator xsdValidator,
            ILogger<XmlSchemaValidationService> logger)
        {
            _xsdValidator = xsdValidator;
            _logger = logger;
        }

        public XsdValidationResult Validate(DisposableList<InputData> inputData, XsdData xsdData)
        {
            var xsdRule = GetXsdRule();
            var startTime = DateTime.Now;
            var codelistUris = new Dictionary<string, Uri>();

            foreach (var data in inputData)
            {
                var result = _xsdValidator.Validate(data.Stream, xsdData);

                data.IsValid = !result.Messages.Any();
                data.Stream.Position = 0;

                result.Messages
                    .Select(message => new RuleMessage { Message = message, Properties = new Dictionary<string, object> { { "FileName", data.FileName } } })
                    .ToList()
                    .ForEach(xsdRule.AddMessage);

                codelistUris.Append(result.CodelistUris);
            }

            xsdRule.Status = !xsdRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(xsdRule, startTime);

            return new XsdValidationResult
            {
                Rule = xsdRule,
                CodelistUris = codelistUris
            };
        }

        private void LogInformation(XsdRule xsdRule, DateTime startTime)
        {
            _logger.LogInformation("{@Rule}", new
            {
                xsdRule.Id,
                xsdRule.Name,
                FullName = xsdRule.ToString(),
                xsdRule.Status,
                TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds,
                MessageCount = xsdRule.Messages.Count
            });
        }

        private static XsdRule GetXsdRule()
        {
            var rule = Activator.CreateInstance(typeof(Skjemavalidering)) as XsdRule;
            rule.Create();

            return rule;
        }
    }
}
