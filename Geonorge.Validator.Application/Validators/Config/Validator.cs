using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Validators.Config
{
    public class Validator
    {
        public ValidatorType ValidatorType { get; init; }
        public string Id { get; init; }
        public IEnumerable<string> XsdVersions { get; init; }
        public Type XsdRuleType { get; init; }
        public IEnumerable<Type> RuleTypes { get; init; }
        public Type ServiceType { get; init; }
        public Type ImplementationType { get; init; }
        public Action<ValidationOptions> ValidationOptions { get; init; }
    }
}
