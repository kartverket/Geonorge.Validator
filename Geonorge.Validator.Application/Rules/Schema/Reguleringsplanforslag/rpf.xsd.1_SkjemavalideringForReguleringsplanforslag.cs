using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Rules.Schema.Reguleringsplanforslag
{
    public class SkjemavalideringForReguleringsplanforslag : SchemaRule
    {
        public override void Create()
        {
            Id = "rpf.xsd.1";
            Name = "Skjemavalidering for reguleringsplanforslag";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
            Documentation = "https://dibk.atlassian.net/wiki/spaces/FP/pages/1855881217/rpf.xsd.k.1";
        }
    }
}
