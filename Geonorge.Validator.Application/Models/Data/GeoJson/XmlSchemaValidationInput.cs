using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.GeoJson
{
    public interface IXmlSchemaValidationInput
    {
        InputData Data { get; }
        Func<InputData, List<string>> Validate { get; }
    }

    public class XmlSchemaValidationInput : IXmlSchemaValidationInput
    {
        public InputData Data { get; }
        public Func<InputData, List<string>> Validate { get; }

        private XmlSchemaValidationInput(Func<InputData, List<string>> validate)
        {
            Validate = validate;
        }

        public static IXmlSchemaValidationInput Create(Func<InputData, List<string>> validate)
        {
            return new XmlSchemaValidationInput(validate);
        }
    }

    public class Skjemavalidering : Rule<IXmlSchemaValidationInput>
    {
        public override void Create()
        {
            Id = "xsd.1";
            Name = "Skjemavalidering";
            Description = "Datasettet må være i henhold til oppgitt applikasjonsskjema";
        }

        protected override void Validate(IXmlSchemaValidationInput input)
        {
            _ = input.Validate(input.Data);
        }
    }
}
