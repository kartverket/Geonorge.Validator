using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Validators.Config
{
    public class ValidatorOptions
    {
        public List<Validator> Validators { get; } = new();
        public Type GetSchemaRuleType(string xmlNamespace) => GetValidator(xmlNamespace)?.XsdRuleType;
        public IEnumerable<Type> GetRuleTypes(string xmlNamespace) => GetValidator(xmlNamespace)?.RuleTypes;
        public ValidatorType GetValidatorType(string xmlNamespace) => GetValidator(xmlNamespace)?.ValidatorType ?? ValidatorType.Undefined;
        public Type GetServiceType(string xmlNamespace) => GetValidator(xmlNamespace)?.ServiceType;
        public Action<ValidationOptions> GetValidationOptions(string xmlNamespace) => GetValidator(xmlNamespace)?.ValidationOptions;
        public Validator GetValidator(string xmlNamespace) => Validators.SingleOrDefault(validator => validator.XmlNamespace == xmlNamespace);

        public void AddValidator<TService, TImplementation>(
            ValidatorType validatorType, string xmlNamespace, IEnumerable<string> xsdVersions, Type xsdRuleType, IEnumerable<Type> ruleTypes, Action<ValidationOptions> options = null)
            where TService : IValidator
            where TImplementation : class, TService
        {
            var allRuleTypes = new List<Type> { xsdRuleType };
            allRuleTypes.AddRange(ruleTypes);

            Validators.Add(new Validator
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                ValidatorType = validatorType,
                XmlNamespace = xmlNamespace,
                XsdVersions = xsdVersions,
                XsdRuleType = xsdRuleType,
                RuleTypes = allRuleTypes,
                ValidationOptions = options
            });
        }
    }
}
