using DiBK.RuleValidator.Config;

namespace Geonorge.Validator.Common.Helpers
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
