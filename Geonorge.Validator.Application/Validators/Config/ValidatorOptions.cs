using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Validators.Config
{
    public class ValidatorOptions
    {
        public List<Validator> Validators { get; } = new();
        public Type GetSchemaRuleType(string id) => GetValidator(id)?.XsdRuleType;
        public IEnumerable<Type> GetRuleTypes(string id) => GetValidator(id)?.RuleTypes;
        public ValidatorType GetValidatorType(string id) => GetValidator(id)?.ValidatorType ?? ValidatorType.Undefined;
        public Type GetServiceType(string id) => GetValidator(id)?.ServiceType;
        public Action<ValidationOptions> GetValidationOptions(string id) => GetValidator(id)?.ValidationOptions;
        public Validator GetValidator(string id) => Validators.SingleOrDefault(validator => validator.Id == id);

        public void AddValidator<TService, TImplementation>(
            ValidatorType validatorType, string id, IEnumerable<string> xsdVersions, Type xsdRuleType, IEnumerable<Type> ruleTypes, Action<ValidationOptions> options = null)
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
                Id = id,
                XsdVersions = xsdVersions,
                XsdRuleType = xsdRuleType,
                RuleTypes = allRuleTypes,
                ValidationOptions = options
            });
        }
    }
}
