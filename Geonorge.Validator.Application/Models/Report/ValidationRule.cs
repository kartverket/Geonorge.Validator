using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Models.Report
{
    public class ValidationRule
    {
        public ValidationRule(Rule rule)
        {
            Id = rule.Id;
            Name = rule.Name;
            Status = rule.Status.ToString();
            PreCondition = rule.PreCondition;
            ChecklistReference = rule.ChecklistReference;
            Description = rule.Description;
            Source = rule.Source;
            Documentation = rule.Documentation;
            MessageType = rule.MessageType.ToString();
            Messages = rule.Messages
                .Select(message =>
                {
                    var messageDictionary = new Dictionary<string, object> { { "Message", message.Message } };
                    messageDictionary.Append(message.Properties);

                    return messageDictionary;
                })
                .ToList();
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public List<Dictionary<string, object>> Messages { get; private set; } = new();
        public string Status { get; private set; }
        public string PreCondition { get; private set; }
        public string ChecklistReference { get; private set; }
        public string Description { get; private set; }
        public string Source { get; private set; }
        public string Documentation { get; private set; }
        public string MessageType { get; private set; }
    }
}
