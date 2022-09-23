using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Rules.XmlSchema
{
    public class Skjemavalidering : XmlSchemaRule
    {
        public override void Create()
        {
            Id = "xsd.1";
            Name = "Skjemavalidering";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }
    }
}
