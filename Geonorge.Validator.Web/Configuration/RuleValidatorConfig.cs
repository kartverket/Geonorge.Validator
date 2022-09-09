using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.Reguleringsplanforslag;
using Reguleringsplanforslag.Rules;
using Reguleringsplanforslag.Rules.Constants;
using Skjemavalidering = Geonorge.Validator.Application.Rules.XmlSchema.Skjemavalidering;

namespace Geonorge.Validator.Web.Configuration
{
    public static class RuleValidatorConfig
    {
        public static void AddRuleValidators(this IServiceCollection services)
        {
            services.AddValidators(options =>
            {
                options.AddValidator<IReguleringsplanforslagValidator, ReguleringsplanforslagValidator>(
                    ValidatorType.Reguleringsplanforslag,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0",
                    new[] { "5.0_rev20210827", "5.0_rev20211104" },
                    typeof(Skjemavalidering),
                    new[] { typeof(IGmlValidationData), typeof(IRpfValidationData) },
                    options =>
                    {
                        options.SkipGroup(RuleGroupId.PlankartOgPlanbestemmelser);
                        options.SkipGroup(RuleGroupId.Planbestemmelser);
                        options.SkipGroup(RuleGroupId.Oversendelse);
                    }
                );
            });
        }
    }
}
