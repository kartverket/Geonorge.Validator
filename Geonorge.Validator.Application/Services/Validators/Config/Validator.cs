using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.Validators.Config
{
    public class Validator
    {
        public ValidatorType ValidatorType { get; init; }
        public string XmlNamespace { get; init; }
        public IEnumerable<Type> RuleTypes { get; init; }
        public Type ServiceType { get; init; }
        public Type ImplementationType { get; init; }
        public IEnumerable<string> AllowedFileTypes { get; init; }
        public Action<ValidationOptions> ValidationOptions { get; init; }
    }
}
