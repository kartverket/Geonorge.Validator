using DiBK.RuleValidator.Config;
using Geonorge.Validator.Application.Models.Data.Validation;

namespace Geonorge.Validator.Application.Rules.GenericGml
{
    public class Setup : IRuleSetup
    {
        public RuleConfig CreateConfig()
        {
            return RuleConfig
                .Create<IGenericGmlValidationData>("Generell GML")
                .AddGroup("GenerellGml", "Generell GML", group => group
                    .AddRule<KodeverdiMåVæreIHenholdTilEksternKodeliste>()
                )
                .Build();
        }
    }
}
