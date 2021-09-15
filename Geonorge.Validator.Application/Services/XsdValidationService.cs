using Arkitektum.XmlSchemaValidator.Validator;
using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Rules.Schema.Planomriss;
using Geonorge.Validator.Application.Rules.Schema.Reguleringsplanforslag;
using Geonorge.Validator.Application.Services.Validators;
using Geonorge.Validator.Application.Services.Validators.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Services
{
    public class XsdValidationService : IXsdValidationService
    {
        private readonly IXmlSchemaValidator _xsdValidator;
        private readonly ValidatorOptions _options;
        private readonly ILogger<XsdValidationService> _logger;

        public XsdValidationService(
            IXmlSchemaValidator xsdValidator,
            IOptions<ValidatorOptions> options,
            ILogger<XsdValidationService> logger)
        {
            _xsdValidator = xsdValidator;
            _options = options.Value;
            _logger = logger;
        }

        public SchemaRule Validate(DisposableList<InputData> inputData, string xmlNamespace)
        {
            var validatorType = _options.GetValidatorType(xmlNamespace);
            var schemaRule = GetSchemaRule(validatorType);
            var startTime = DateTime.Now;

            foreach (var data in inputData)
            {
                var messages = _xsdValidator.Validate(validatorType.ToString(), data.Stream);

                data.IsValid = !messages.Any();
                data.Stream.Seek(0, SeekOrigin.Begin);

                schemaRule.Messages.AddRange(messages
                    .Select(message => new RuleMessage { Message = message, FileName = data.FileName }));
            }

            schemaRule.Status = !schemaRule.Messages.Any() ? Status.PASSED : Status.FAILED;

            LogInformation(schemaRule, startTime);

            return schemaRule;
        }

        private void LogInformation(SchemaRule schemaRule, DateTime startTime)
        {
            _logger.LogInformation("{@Rule}", new
            {
                schemaRule.Id,
                schemaRule.Name,
                FullName = schemaRule.ToString(),
                schemaRule.Status,
                TimeUsed = DateTime.Now.Subtract(startTime).TotalSeconds,
                MessageCount = schemaRule.Messages.Count
            });
        }

        private static SchemaRule GetSchemaRule(ValidatorType dataType)
        {
            var type = GetRuleTypeFromDataType(dataType);

            if (type == null)
                return null;

            var rule = Activator.CreateInstance(type) as SchemaRule;
            rule.Create();

            return rule;
        }

        private static Type GetRuleTypeFromDataType(ValidatorType validatorType)
        {
            return validatorType switch
            {
                ValidatorType.Planomriss => typeof(SkjemavalideringForGmlPlanomriss),
                ValidatorType.Reguleringsplanforslag => typeof(SkjemavalideringForReguleringsplanforslag),
                _ => throw new Exception("Could not find schema rule!")
            };
        }
    }
}
