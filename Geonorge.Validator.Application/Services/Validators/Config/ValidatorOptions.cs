using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Services.Validators.Config
{
    public class ValidatorOptions
    {
        public List<Validator> Validators { get; } = new();
        public IEnumerable<Type> GetRuleTypes(string xmlNamespace) => GetValidator(xmlNamespace)?.RuleTypes;
        public ValidatorType GetValidatorType(string xmlNamespace) => GetValidator(xmlNamespace)?.ValidatorType ?? ValidatorType.Undefined;
        public Type GetServiceType(string xmlNamespace) => GetValidator(xmlNamespace)?.ServiceType;
        public IEnumerable<string> GetAllowedFileTypes(string xmlNamespace) => GetValidator(xmlNamespace)?.AllowedFileTypes;
        public Action<ValidationOptions> GetValidationOptions(string xmlNamespace) => GetValidator(xmlNamespace)?.ValidationOptions;
        public Validator GetValidator(string xmlNamespace) => Validators.SingleOrDefault(validator => validator.XmlNamespace == xmlNamespace);

        public void AddValidator<TService, TImplementation>(
            ValidatorType validatorType, string xmlNamespace, IEnumerable<Type> ruleTypes, IEnumerable<string> allowedFileTypes, Action<ValidationOptions> options = null)
            where TService : IValidator
            where TImplementation : class, TService
        {
            Validators.Add(new Validator
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                ValidatorType = validatorType,
                XmlNamespace = xmlNamespace,
                RuleTypes = ruleTypes,
                AllowedFileTypes = allowedFileTypes ?? new[] { ".xml" },
                ValidationOptions = options
            });
        }
    }
}
