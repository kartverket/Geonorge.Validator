using DiBK.RuleValidator;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Config;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.XsdValidator.Config;
using Geonorge.XsdValidator.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RuleInfo = DiBK.RuleValidator.RuleInfo;
using RuleInformation = Geonorge.Validator.Application.Models.Config.RuleInfo;
using RuleSet = Geonorge.Validator.Application.Models.Data.RuleSet;

namespace Geonorge.Validator.Application.Services.RuleSetService
{
    public class RuleSetService : IRuleSetService
    {
        private static readonly Assembly _xsdRuleAssembly = Assembly.GetExecutingAssembly();

        private readonly IRuleValidator _ruleValidator;
        private readonly IMultipartRequestService _multipartRequestService;
        private readonly IXsdHttpClient _xsdHttpClient;
        private readonly IOptions<ValidatorOptions> _validatorOptions;
        private readonly RuleInfoOptions _ruleInfoOptions;
        private readonly string _xsdCacheFilesPath;

        public RuleSetService(
            IRuleValidator ruleValidator,
            IMultipartRequestService multipartRequestService,
            IXsdHttpClient xsdHttpClient,
            IOptions<XsdValidatorSettings> xsdValidatorOptions,
            IOptions<ValidatorOptions> validatorOptions,
            RuleInfoOptions ruleInfoOptions)
        {
            _ruleValidator = ruleValidator;
            _multipartRequestService = multipartRequestService;
            _xsdHttpClient = xsdHttpClient;
            _xsdCacheFilesPath = xsdValidatorOptions.Value.CacheFilesPath;
            _validatorOptions = validatorOptions;
            _ruleInfoOptions = ruleInfoOptions;
        }

        public List<RuleSet> GetRuleSets()
        {
            var ruleSets = _ruleInfoOptions.RuleInfo
                .Select(CreateRuleSet)
                .ToList();

            var xsdRuleSet = CreateRuleSetForXsdRules();

            if (xsdRuleSet != null)
                ruleSets.Insert(0, xsdRuleSet);

            return ruleSets;
        }

        public async Task<List<RuleSet>> GetRuleSetsForNamespace()
        {
            var submittal = await _multipartRequestService.GetFilesFromMultipart();

            if (submittal == null || !submittal.Files.Any())
                return null;

            var xsdData = await GetXsdDataAsync(submittal.Files, submittal.Schema);
            var xmlMetadata = await XmlMetadata.CreateAsync(xsdData.Streams[0], _xsdCacheFilesPath);
            var validator = _validatorOptions.Value.GetValidator(xmlMetadata.Namespace);
            var ruleSets = new List<RuleSet>();

            if (validator != null && validator.XsdVersions.Contains(xmlMetadata.XsdVersion))
            {
                foreach (var ruleType in validator.RuleTypes)
                {
                    var ruleInfo = _ruleInfoOptions.RuleInfo
                        .SingleOrDefault(ruleInfo => ruleInfo.RuleType == ruleType);

                    if (ruleInfo != null)
                        ruleSets.Add(CreateRuleSet(ruleInfo));
                }
            }
            else if (xmlMetadata.IsGml32)
            {
                var genericRuleInfo = _ruleInfoOptions.RuleInfo
                    .SingleOrDefault(ruleInfo => ruleInfo.RuleType == typeof(IGenericGmlValidationData));

                ruleSets.Add(CreateRuleSet(genericRuleInfo));

                var gmlRuleInfo = _ruleInfoOptions.RuleInfo
                    .SingleOrDefault(ruleInfo => ruleInfo.RuleType == typeof(IGmlValidationData));

                ruleSets.Add(CreateRuleSet(gmlRuleInfo));
            }

            var xsdRuleSet = CreateRuleSetForXsdRules();

            if (xsdRuleSet != null)
                ruleSets.Insert(0, xsdRuleSet);

            return ruleSets;
        }

        private async Task<XsdData> GetXsdDataAsync(List<IFormFile> xmlFiles, IFormFile xsdFile)
        {
            if (xsdFile == null)
                return await _xsdHttpClient.GetXsdFromXmlFilesAsync(xmlFiles);

            var xsdData = new XsdData();
            xsdData.Streams.Add(xsdFile.OpenReadStream());

            return xsdData;
        }

        private RuleSet CreateRuleSet(RuleInformation ruleInfo)
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
                Groups = new List<RuleSetGroup> { new RuleSetGroup { Rules = ruleInfos, GroupId = "Applikasjonsskjema" } },
                Mandatory = true
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
