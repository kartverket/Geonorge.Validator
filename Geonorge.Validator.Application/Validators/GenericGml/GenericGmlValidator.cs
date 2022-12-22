using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.CodelistResolver;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.XmlSchema.Config;
using Geonorge.Validator.XmlSchema.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using static Geonorge.Validator.Common.Helpers.GmlHelper;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public class GenericGmlValidator : IGenericGmlValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly ICodelistResolverHttpClient _codelistResolverHttpClient;
        private readonly INotificationService _notificationService;
        private readonly XmlSchemaValidatorSettings _settings;

        public GenericGmlValidator(
            IRuleValidator validator,
            ICodelistHttpClient codelistHttpClient,
            ICodelistResolverHttpClient codelistResolverHttpClient,
            INotificationService notificationService,
            IOptions<XmlSchemaValidatorSettings> options)
        {
            _validator = validator;
            _codelistHttpClient = codelistHttpClient;
            _codelistResolverHttpClient = codelistResolverHttpClient;
            _notificationService = notificationService;
            _settings = options.Value;
        }

        public async Task<List<Rule>> Validate(
            DisposableList<InputData> inputData, HashSet<XmlSchemaElement> xmlSchemaElements, XmlSchemaSet xmlSchemaSet, List<string> skipRules)
        {
            await _notificationService.SendAsync("Bearbeider data");

            var gmlValidationInputV1 = await GetGmlValidationInputV1(inputData);
            //var codeSpaces = await _codelistHttpClient.GetGmlCodeSpacesAsync(codelistUris);

            var codeElements = xmlSchemaElements
                .Where(element => _settings.CodelistSelectors.Any(selector => selector.QualifiedName.Equals(element.SchemaTypeName)))
                .GroupBy(element => element.QualifiedName)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.ToList());

            var codeListDict = new Dictionary<XName, Uri>();

            foreach (var (qualifiedName, elements) in codeElements)
            {
                XNamespace ns = qualifiedName.Namespace;
                XName name = ns + qualifiedName.Name;
                
                var uris = new HashSet<Uri>();

                foreach (var element in elements)
                {
                    var selector = _settings.CodelistSelectors
                        .SingleOrDefault(selector => selector.QualifiedName.Equals(element.SchemaTypeName));

                    uris.Add(selector.UriResolver.Invoke(element));
                }

                var uri = uris
                    .Where(uri => uri != null)
                    .FirstOrDefault();

                if (uri != null)
                    codeListDict.Add(name, uri);
            }


            var gmlValidationInputV2 = GmlValidationInputV2.Create(
                gmlValidationInputV1.Surfaces, 
                gmlValidationInputV1.Solids,
                null,
                null
            );

            await _notificationService.SendAsync("Validerer");

            await _validator.Validate(gmlValidationInputV1, options =>
            {                            
                skipRules.ForEach(options.SkipRule);
                options.OnRuleExecuted = OnRuleExecuted;
            });
            
            await _validator.Validate(gmlValidationInputV2, options =>
            {
                skipRules.ForEach(options.SkipRule);
                options.OnRuleExecuted = OnRuleExecuted;
            });

            gmlValidationInputV1.Dispose();
            gmlValidationInputV2.Dispose();

            await _notificationService.SendAsync("Lager rapport");

            return _validator.GetAllRules();
        }

        private async Task OnRuleExecuted(RuleResult result)
        {
            await _notificationService.SendAsync($"{result} ({result.TimeUsed:0.##} sek.)");
        }

        private static async Task<IGmlValidationInputV1> GetGmlValidationInputV1(DisposableList<InputData> inputData)
        {
            var gmlDocuments2D = new List<GmlDocument>();
            var gmlDocuments3D = new List<GmlDocument>();

            foreach (var data in inputData)
            {
                if (!data.IsValid)
                    continue;

                var document = GmlDocument.Create(data);
                var dimensions = await GetDimensionsAsync(data.Stream);

                if (dimensions == 2)
                    gmlDocuments2D.Add(document);
                else if (dimensions == 3)
                    gmlDocuments3D.Add(document);
            }

            return GmlValidationInput.Create(
                gmlDocuments2D,
                gmlDocuments3D
            );
        }
    }
}
