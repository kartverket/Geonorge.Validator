using DiBK.RuleValidator.Config;
using Geonorge.Validator.Rules.GeoJson;

namespace Geonorge.Validator.Application.Rules.GeoJson
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<IGeoJsonValidationInput>("Generell GeoJSON")
                .AddGroup("GenerellGeoJson", "Generell GeoJSON", group => group
                    .AddRule<LinjerSkalHaGyldigGeometri>()
                    .AddRule<LinjerKanIkkeHaDobbeltpunkter>()
                    .AddRule<FlaterSkalHaGyldigGeometri>()
                    .AddRule<AvgrensningenTilEnFlateKanIkkeKrysseSegSelv>()
                )
                .Build();
        }
    }
}
