using DiBK.RuleValidator;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class RuleSet
    {
        public string Name { get; set; }
        public List<RuleSetGroup> Groups { get; set; }
        public bool Mandatory { get; set; }
    }
}
