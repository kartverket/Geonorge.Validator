using DiBK.RuleValidator.Config;
using System;

namespace Geonorge.Validator.Application.Models.Config
{
    public class RuleInfo
    {
        public string Name { get; set; }
        public Type RuleType { get; set; }
        public Action<ValidationOptions> Options { get; set; }
    }
}
