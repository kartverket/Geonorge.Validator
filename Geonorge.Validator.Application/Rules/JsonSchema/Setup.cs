using DiBK.RuleValidator.Config;
using Geonorge.Validator.Application.Models.Data.Json;

namespace Geonorge.Validator.Application.Rules.JsonSchema
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<IJsonSchemaValidationInput>("Applikasjonsskjema")
                .AddGroup("Applikasjonsskjema", "Applikasjonsskjema", group => group
                    .AddRule<SkjemavalideringJson>()
                )
                .Build();
        }
    }
}
