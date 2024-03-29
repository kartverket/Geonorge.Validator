﻿using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.Reguleringsplanforslag;
using Innsending.Planforslag.Rules;
using Innsending.Planforslag.Rules.Constants;
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
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/20230701",
                    new[] { "20230701" },
                    typeof(Skjemavalidering),
                    new[] { typeof(IGmlValidationInputV1), typeof(IRpfValidationInput) },
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
