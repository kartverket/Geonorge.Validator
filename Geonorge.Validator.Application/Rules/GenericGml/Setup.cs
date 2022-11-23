using DiBK.RuleValidator.Config;
using Geonorge.Validator.Application.Models.Data.Validation;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<IGmlValidationInputV2>("Generell GML v2")
                .AddGroup("GenerellGmlV2", "Generell GML v2", group => group
                    .AddRule<KodeverdiMåVæreIHenholdTilEksternKodeliste>()
                    .AddRule<FungerendeReferanser>()
                )
                .Build();
        }
    }
}
