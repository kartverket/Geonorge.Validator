using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.HttpClients.XmlSchema;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Config;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Validators.Config;
using Geonorge.Validator.Common.Exceptions;
using Geonorge.Validator.Common.Helpers;
using Geonorge.Validator.Common.Models;
using Geonorge.Validator.GeoJson.Helpers;
using Geonorge.Validator.Rules.GeoJson;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using _Validator = Geonorge.Validator.Application.Validators.Config.Validator;
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
        private readonly IXmlSchemaHttpClient _xmlSchemaHttpClient;
        private readonly IJsonSchemaHttpClient _jsonSchemaHttpClient;
        private readonly IOptions<ValidatorOptions> _validatorOptions;
        private readonly RuleInfoOptions _ruleInfoOptions;

        public RuleSetService(
            IRuleValidator ruleValidator,
            IMultipartRequestService multipartRequestService,
            IXmlSchemaHttpClient xsdHttpClient,
            IJsonSchemaHttpClient jsonSchemaHttpClient,
            IOptions<ValidatorOptions> validatorOptions,
            RuleInfoOptions ruleInfoOptions)
        {
            _ruleValidator = ruleValidator;
            _multipartRequestService = multipartRequestService;
            _xmlSchemaHttpClient = xsdHttpClient;
            _jsonSchemaHttpClient = jsonSchemaHttpClient;
            _validatorOptions = validatorOptions;
            _ruleInfoOptions = ruleInfoOptions;
        }

        public List<RuleSet> GetRuleSets()
        {
            var ruleSets = _ruleInfoOptions.RuleInfo
                .Select(CreateRuleSet)
                .ToList();

            var jsonSchemaRule = GetJsonSchemaRule();
            var xsdRuleSet = CreateRuleSetForSchemaRules(new Rule[] { GetXmlSchemaRule(), GetJsonSchemaRule() });

            if (xsdRuleSet != null)
                ruleSets.Insert(0, xsdRuleSet);

            return ruleSets;
        }

        public async Task<List<RuleSet>> GetRuleSetsForDataset()
        {
            var submittal = await _multipartRequestService.GetFilesFromMultipartAsync();

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
            var xmlSchemaData = await GetXmlSchemaDataAsync(submittal);
            var xmlMetadata = await XmlMetadata.CreateAsync(submittal.InputData.First().Stream, xmlSchemaData.Streams);
            var validators = GetValidators(xmlMetadata);
            var ruleSets = new List<RuleSet>();

            if (validators.Any())
            {
                foreach (var validator in validators)
                {
                    foreach (var ruleType in validator.RuleTypes)
                    {
                        var ruleInfo = _ruleInfoOptions.RuleInfo
                            .SingleOrDefault(ruleInfo => ruleInfo.RuleType == ruleType);

                        if (ruleInfo != null)
                            ruleSets.Add(CreateRuleSet(ruleInfo));
                    }
                }
            }
            else if (xmlMetadata.IsGml32)
            {
                var gmlV1RuleInfo = _ruleInfoOptions.RuleInfo
                    .SingleOrDefault(ruleInfo => ruleInfo.RuleType == typeof(IGmlValidationInputV1));

                ruleSets.Add(CreateRuleSet(gmlV1RuleInfo));

                var gmlV2RuleInfo = _ruleInfoOptions.RuleInfo
                    .SingleOrDefault(ruleInfo => ruleInfo.RuleType == typeof(IGmlValidationInputV2));

                ruleSets.Add(CreateRuleSet(gmlV2RuleInfo));
            }

            var xmlSchemaRuleSet = CreateRuleSetForSchemaRules(new[] { GetXmlSchemaRule() });

            if (xmlSchemaRuleSet != null)
                ruleSets.Insert(0, xmlSchemaRuleSet);

            return ruleSets;
        }

        private async Task<List<RuleSet>> GetRuleSetsForJson(Submittal submittal)
        {
            var ruleSets = new List<RuleSet>();
            var schema = await _jsonSchemaHttpClient.GetJsonSchemaAsync(submittal.InputData, submittal.Schema);

            if (await IsGeoJsonAsync(submittal.InputData))
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

        private async Task<XmlSchemaData> GetXmlSchemaDataAsync(Submittal submittal)
        {
            if (submittal.SchemaUri != null)
            {
                var stream = await _xmlSchemaHttpClient.FetchXmlSchemaAsync(submittal.SchemaUri);
                var xmlSchemaData = new XmlSchemaData();
                xmlSchemaData.Streams.Add(stream);

                return xmlSchemaData;
            }
            else if (submittal.Schema != null)
            {
                var xmlSchemaData = new XmlSchemaData();
                xmlSchemaData.Streams.Add(submittal.Schema);

                return xmlSchemaData;
            }
            else
            {
                return await _xmlSchemaHttpClient.GetXmlSchemaFromInputDataAsync(submittal.InputData);
            }
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

        private List<_Validator> GetValidators(XmlMetadata xmlMetadata)
        {
            return xmlMetadata.Namespaces
                .Select(@namespace => (Namespace: @namespace, Validator: _validatorOptions.Value.GetValidator(@namespace.Namespace)))
                .Where(tuple => tuple.Validator != null &&
                    tuple.Validator.XsdVersions
                        .Any(xsdVersion => xmlMetadata.Namespaces
                            .Any(@namespace => @namespace.XsdVersion.Equals(xsdVersion)))
                )
                .Select(tuple => tuple.Validator)
                .ToList();
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

        private static async Task<bool> IsGeoJsonAsync(DisposableList<InputData> inputData)
        {
            foreach (var data in inputData)
            {
                var document = await JsonHelper.LoadJsonDocumentAsync(data.Stream);

                if (document.IsValid(GeoJsonHelper.GeoJsonSchema))
                    return true;
            }

            return false;
        }

        private static XmlSchemaRule GetXmlSchemaRule()
        {
            return _schemaRuleAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(XmlSchemaRule)) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as XmlSchemaRule;
                    rule.Create();

                    return rule;
                })
                .FirstOrDefault();
        }

        private static JsonSchemaRule GetJsonSchemaRule()
        {
            return _schemaRuleAssembly.GetTypes()
                .Where(type => type.IsSubclassOf(typeof(JsonSchemaRule)) &&
                    type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type =>
                {
                    var rule = Activator.CreateInstance(type) as JsonSchemaRule;
                    rule.Create();

                    return rule;
                })
                .FirstOrDefault();
        }
    }
}
