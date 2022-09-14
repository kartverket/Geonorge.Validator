using DiBK.RuleValidator;
using Geonorge.Validator.Application.Models.Data.Json;
using Geonorge.Validator.GeoJson.Extensions;

namespace Geonorge.Validator.Application.Rules.JsonSchema
{
    public class SkjemavalideringJson : Rule<IJsonSchemaValidationInput>
    {
        public override void Create()
        {
            Id = "json.sch.1";
            Name = "Skjemavalidering JSON";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }

        protected override void Validate(IJsonSchemaValidationInput input)
        {
            foreach (var inputData in input.Data)
            {
                var validationErrors = input.Validate(inputData);

                foreach (var error in validationErrors)
                {
                    this.AddMessage(
                        error.Message,
                        inputData.FileName,
                        error.JsonPath,
                        error.LineNumber,
                        error.LinePosition
                    );
                }
            }
        }
    }
}
