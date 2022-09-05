using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Utils
{
    public class ValidationHelper
    {
        public static ValidationOptions CreateValidationOptions(Action<ValidationOptions> options, List<string> skipRules)
        {
            var validationOptions = new ValidationOptions();
            options?.Invoke(validationOptions);
            skipRules.ForEach(validationOptions.SkipRule);

            return validationOptions;
        }
    }
}
