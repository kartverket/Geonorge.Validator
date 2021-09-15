using DiBK.RuleValidator.Config;

namespace Geonorge.Validator.Application.Rules.Schema.Reguleringsplanforslag
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<SkjemavalideringForReguleringsplanforslag>("Applikasjonsskjema")
                .AddGroup("Applikasjonsskjema", "Applikasjonsskjema", group => group
                    .AddRule<SkjemavalideringForReguleringsplanforslag>()
                )
                .Build();
        }
    }
}
