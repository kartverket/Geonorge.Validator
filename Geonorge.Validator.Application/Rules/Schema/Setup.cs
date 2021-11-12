using DiBK.RuleValidator.Config;

namespace Geonorge.Validator.Application.Rules.Schema
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<Skjemavalidering>("Applikasjonsskjema")
                .AddGroup("Applikasjonsskjema", "Applikasjonsskjema", group => group
                    .AddRule<Skjemavalidering>()
                )
                .Build();
        }
    }
}
