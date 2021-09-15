using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Rules.Schema.Planomriss
{
    public class SkjemavalideringForGmlPlanomriss : SchemaRule
    {
        public override void Create()
        {
            Id = "po.xsd.1";
            Name = "Skjemavalidering for GML-planomriss";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }
    }
}
