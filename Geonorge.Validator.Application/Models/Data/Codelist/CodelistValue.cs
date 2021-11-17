namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class CodelistValue
    {
        public string Codevalue { get; private set; }
        public string Label { get; private set; }
        public string Description { get; private set; }

        public CodelistValue(string codevalue, string label, string description)
        {
            Codevalue = codevalue;
            Label = label;
            Description = description;
        }
    }
}
