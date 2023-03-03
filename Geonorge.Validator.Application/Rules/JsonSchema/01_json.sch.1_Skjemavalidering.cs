using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Rules.JsonSchema
{
    public class SkjemavalideringJson : JsonSchemaRule
    {
        public override void Create()
        {
            Id = "json.sch.1";
            Name = "Skjemavalidering JSON";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }
    }
}
