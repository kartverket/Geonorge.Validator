using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Rules.Schema;
using Geonorge.Validator.Application.Validators;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Application.Validators.Plangrense;
using Geonorge.Validator.Application.Validators.Reguleringsplanforslag;
using Microsoft.Extensions.DependencyInjection;
using Reguleringsplanforslag.Rules;
using Reguleringsplanforslag.Rules.Constants;

namespace Geonorge.Validator.Web.Configuration
{
    public static class RuleValidatorConfig
    {
        public static void AddValidators(this IServiceCollection services)
        {
            services.AddValidators(options =>
            {
                options.AddValidator<IReguleringsplanforslagValidator, ReguleringsplanforslagValidator>(
                    ValidatorType.Reguleringsplanforslag,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0",
                    new[] { "5.0_rev20210827", "5.0_rev20211104" },
                    new[] { ".gml" },
                    typeof(Skjemavalidering),
                    new[] { typeof(IGmlValidationData), typeof(IRpfValidationData) },
                    options =>
                    {
                        options.SkipGroup(RuleGroupId.PlankartOgPlanbestemmelser);
                        options.SkipGroup(RuleGroupId.Planbestemmelser);
                        options.SkipGroup(RuleGroupId.Oversendelse);
                    }
                );

                options.AddValidator<IPlangrenseValidator, PlangrenseValidator>(
                    ValidatorType.Plangrense,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0/Planomriss",
                    new[] { "5.0_rev20210608" },
                    new[] { ".gml" },
                    typeof(Skjemavalidering),
                    new[] { typeof(IGmlValidationData) },
                    options =>
                    {
                        options.SkipRule<KoordinatreferansesystemForKart3D>();
                        options.SkipRule<KurverSkalHaGyldigGeometri>();
                        options.SkipRule<BueKanIkkeHaDobbeltpunkter>();
                        options.SkipRule<BueKanIkkeHaPunkterPåRettLinje>();
                    }
                );
            });
        }
    }
}
