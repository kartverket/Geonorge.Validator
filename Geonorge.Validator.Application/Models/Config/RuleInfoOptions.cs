using DiBK.RuleValidator.Config;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Config
{

    public class RuleInfoOptions
    {
        public List<RuleInfo> RuleInfo { get; } = new();

        public void AddRuleInformation<T>(string name, Action<ValidationOptions> options = null) where T : class
        {
            RuleInfo.Add(new() { Name = name, RuleType = typeof(T), Options = options });
        }
    }
}
