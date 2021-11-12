using Geonorge.Validator.Application.Models;

namespace Geonorge.Validator.Application.Rules.Schema
{
    public class Skjemavalidering : XsdRule
    {
        public override void Create()
        {
            Id = "xsd.1";
            Name = "Skjemavalidering";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }
    }
}
