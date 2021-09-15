using DiBK.RuleValidator.Config;

namespace Geonorge.Validator.Application.Rules.Schema.Planomriss
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<SkjemavalideringForGmlPlanomriss>("Applikasjonsskjema")
                .AddGroup("Applikasjonsskjema", "Applikasjonsskjema", group => group
                    .AddRule<SkjemavalideringForGmlPlanomriss>()
                )
                .Build();
        }
    }
}
