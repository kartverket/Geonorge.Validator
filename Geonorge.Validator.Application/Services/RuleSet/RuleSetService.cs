using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Exceptions;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.HttpClients.XmlSchema;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Config;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Models.Data.Json;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Common.Models;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.Rules.GeoJson;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly Assembly _schemaRuleAssembly = Assembly.GetExecutingAssembly();

        private readonly IRuleValidator _ruleValidator;
        private readonly IMultipartRequestService _multipartRequestService;
        private readonly IXmlSchemaHttpClient _xsdHttpClient;
        private readonly IJsonSchemaHttpClient _jsonSchemaHttpClient;
        private readonly IOptions<ValidatorOptions> _validatorOptions;
        private readonly RuleInfoOptions _ruleInfoOptions;
        private readonly string _xsdCacheFilesPath;

        public RuleSetService(
            IRuleValidator ruleValidator,
            IMultipartRequestService multipartRequestService,
            IXmlSchemaHttpClient xsdHttpClient,
            IJsonSchemaHttpClient jsonSchemaHttpClient,
            IOptions<XsdValidatorSettings> xsdValidatorOptions,
            IOptions<ValidatorOptions> validatorOptions,
            RuleInfoOptions ruleInfoOptions)
        {
            _ruleValidator = ruleValidator;
            _multipartRequestService = multipartRequestService;
            _xsdHttpClient = xsdHttpClient;
            _jsonSchemaHttpClient = jsonSchemaHttpClient;
            _xsdCacheFilesPath = xsdValidatorOptions.Value.CacheFilesPath;
            _validatorOptions = validatorOptions;
            _ruleInfoOptions = ruleInfoOptions;
        }

        public List<RuleSet> GetRuleSets()
        {
            var ruleSets = _ruleInfoOptions.RuleInfo
                .Select(CreateRuleSet)
                .ToList();

            var jsonSchemaRule = GetJsonSchemaRule();
            var xsdRuleSet = CreateRuleSetForSchemaRules(new Rule[] { GetXsdRule(), GetJsonSchemaRule() });

            if (xsdRuleSet != null)
                ruleSets.Insert(0, xsdRuleSet);

            return ruleSets;
        }

        public async Task<List<RuleSet>> GetRuleSetsForDataset()
        {
            var submittal = await _multipartRequestService.GetFilesFromMultipart();

            if (!submittal.IsValid)
                throw new MultipartRequestException("Datasettet inneholder ugyldige filer.");

            return submittal.FileType switch
            {
                FileType.GML32 or FileType.XML => await GetRuleSetsForXml(submittal),
                FileType.JSON => await GetRuleSetsForJson(submittal),
                _ => throw new MultipartRequestException("Datasettet inneholder ukjente filtyper."),
            };
        }

        private async Task<List<RuleSet>> GetRuleSetsForXml(Submittal submittal)
        {
            var xsdData = await GetXsdDataAsync(submittal.InputData, submittal.Schema);
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

            var xsdRuleSet = CreateRuleSetForSchemaRules(new[] { GetXsdRule() });

            if (xsdRuleSet != null)
                ruleSets.Insert(0, xsdRuleSet);

            return ruleSets;
        }

        private async Task<List<RuleSet>> GetRuleSetsForJson(Submittal submittal)
        {
            var ruleSets = new List<RuleSet>();
            var schema = await _jsonSchemaHttpClient.GetJsonSchemaAsync(submittal.InputData, submittal.Schema);

            if (GeoJsonHelper.HasGeoJson(schema))
            {
                var genericGeoJsonRuleInfo = _ruleInfoOptions.RuleInfo
                    .SingleOrDefault(ruleInfo => ruleInfo.RuleType == typeof(IGeoJsonValidationInput));

                ruleSets.Add(CreateRuleSet(genericGeoJsonRuleInfo));
            }

            var schemaRule = GetJsonSchemaRule();
            var ruleSet = CreateRuleSetForSchemaRules(new[] { schemaRule });
            ruleSets.Insert(0, ruleSet);

            return ruleSets;
        }

        private async Task<XmlSchemaData> GetXsdDataAsync(DisposableList<InputData> inputData, Stream schema)
        {
            if (schema == null)
                return await _xsdHttpClient.GetXmlSchemaFromInputDataAsync(inputData);

            var xsdData = new XmlSchemaData();
            xsdData.Streams.Add(schema);

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

        private static RuleSet CreateRuleSetForSchemaRules(IEnumerable<Rule> rules)
        {
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

        private static Rule<IJsonSchemaValidationInput> GetJsonSchemaRule()
        {
            return _schemaRuleAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(Rule<IJsonSchemaValidationInput>)) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as Rule<IJsonSchemaValidationInput>;
                    rule.Create();

                    return rule;
                })
                .FirstOrDefault();
        }

        private static XsdRule GetXsdRule()
        {
            return _schemaRuleAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(XsdRule)) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as XsdRule;
                    rule.Create();

                    return rule;
                })
                .FirstOrDefault();
        }
    }
}
