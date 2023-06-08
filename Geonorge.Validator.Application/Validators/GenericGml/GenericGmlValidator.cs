using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Schema;
using static Geonorge.Validator.Common.Helpers.GmlHelper;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public class GenericGmlValidator : IGenericGmlValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly INotificationService _notificationService;

        public GenericGmlValidator(
            IRuleValidator validator,
            ICodelistHttpClient codelistHttpClient,
            INotificationService notificationService)
        {
            _validator = validator;
            _codelistHttpClient = codelistHttpClient;
            _notificationService = notificationService;
        }

        public async Task<List<Rule>> Validate(
            DisposableList<InputData> inputData, 
            HashSet<XmlSchemaElement> xmlSchemaElements,
            Dictionary<string, Dictionary<XmlLineInfo, XmlSchemaLineInfo>> xmlSchemaMappings,
            XmlSchemaSet xmlSchemaSet, 
            List<string> skipRules)
        {
            await _notificationService.SendAsync("Bearbeider data");

            var gmlValidationInputV1 = await GetGmlValidationInputV1(inputData);

            var gmlValidationInputV2 = GmlValidationInputV2.Create(
                gmlValidationInputV1.Surfaces, 
                gmlValidationInputV1.Solids,
                new XLinkValidator(xmlSchemaSet, xmlSchemaElements, xmlSchemaMappings, uri => _codelistHttpClient.GetCodelistAsync(uri))
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
