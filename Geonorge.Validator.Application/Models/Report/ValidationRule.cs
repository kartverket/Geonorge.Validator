using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Report
{
    public class ValidationRule
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ValidationRuleMessage> Messages { get; set; } = new();
        public string Status { get; set; }
        public string PreCondition { get; set; }
        public string ChecklistReference { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }
        public string Documentation { get; set; }
        public string MessageType { get; set; }
    }
}
