using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Rules.Schema;
using Geonorge.XsdValidator.Validator;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Services.XsdValidation
{
    public class XsdValidationService : IXsdValidationService
    {
        private readonly IXsdValidator _xsdValidator;
        private readonly ILogger<XsdValidationService> _logger;

        public XsdValidationService(
            IXsdValidator xsdValidator,
            ILogger<XsdValidationService> logger)
        {
            _xsdValidator = xsdValidator;
            _logger = logger;
        }

        public XsdRule Validate(DisposableList<InputData> inputData, Stream xsdStream)
        {
            var xsdRule = GetXsdRule();
            var startTime = DateTime.Now;

            foreach (var data in inputData)
            {
                var messages = _xsdValidator.Validate(data.Stream, xsdStream);

                data.IsValid = !messages.Any();
                data.Stream.Position = 0;

                messages
                    .Select(message => new RuleMessage { Message = message, Properties = new Dictionary<string, object> { { "FileName", data.FileName } } })
                    .ToList()
                    .ForEach(xsdRule.AddMessage);
            }

            xsdRule.Status = !xsdRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(xsdRule, startTime);

            return xsdRule;
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
