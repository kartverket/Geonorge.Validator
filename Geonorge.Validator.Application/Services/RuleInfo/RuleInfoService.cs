using DiBK.RuleValidator;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RuleSet = Geonorge.Validator.Application.Models.Data.RuleSet;
using RuleInfo = DiBK.RuleValidator.RuleInfo;

namespace Geonorge.Validator.Application.Services.RuleInfoService
{
    public class RuleInfoService : IRuleInfoService
    {
        private static readonly Assembly _xsdRuleAssembly = Assembly.GetExecutingAssembly();
        private readonly IRuleValidator _ruleValidator;
        private readonly RuleInfoOptions _options;

        public RuleInfoService(
            IRuleValidator ruleValidator,
            RuleInfoOptions options)
        {
            _ruleValidator = ruleValidator;
            _options = options;
        }

        public List<RuleSet> GetRuleInfo()
        {
            var ruleInformation = _options.RuleInfo
                .Select(ruleInfo =>
                {
                    var name = ruleInfo.Name;
                    var groups = _ruleValidator.GetRuleInfo(new[] { ruleInfo.RuleType }, ruleInfo.Options);

                    if (groups.Count == 1 && groups[0].Name == name)
                        groups[0].Name = null;

                    return new RuleSet
                    {
                        Name = name,
                        Groups = groups
                    };
                })
                .ToList();

            var xsdRuleSet = CreateRuleSetForXsdRules();

            if (xsdRuleSet != null)
                ruleInformation.Insert(0, xsdRuleSet);

            return ruleInformation;
        }

        private static RuleSet CreateRuleSetForXsdRules()
        {
            var rules = GetXsdRules();

            if (!rules.Any())
                return null;

            var ruleInfos = rules
                .Select(rule => new RuleInfo(rule.Id, rule.Name, rule.Description, rule.MessageType.ToString(), rule.Documentation))
                .ToList();

            return new RuleSet
            {
                Name = "Applikasjonsskjema",
                Groups = new List<RuleSetGroup> { new RuleSetGroup { Rules = ruleInfos } }
            };
        }

        private static List<XsdRule> GetXsdRules()
        {
            return _xsdRuleAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(XsdRule)) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as XsdRule;
                    rule.Create();

                    return rule;
                })
                .ToList();
        }
    }
}
