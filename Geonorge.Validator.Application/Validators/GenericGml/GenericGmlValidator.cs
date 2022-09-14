using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Notification;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public async Task<List<Rule>> Validate(DisposableList<InputData> inputData, Dictionary<string, Uri> codelistUris, List<string> skipRules)
        {
            await _notificationService.SendAsync("Bearbeider data");

            var gmlValidationData = await GetGmlValidationData(inputData);
            var codeSpaces = await _codelistHttpClient.GetGmlCodeSpacesAsync(codelistUris);

            var genericGmlValidationData = GenericGmlValidationData.Create(
                gmlValidationData.Surfaces, 
                gmlValidationData.Solids,
                codeSpaces
            );

            await _notificationService.SendAsync("Validerer");

            await _validator.Validate(genericGmlValidationData, options => 
            {
                skipRules.ForEach(options.SkipRule);
                options.OnRuleExecuted = OnRuleExecuted;
            });

            await _validator.Validate(gmlValidationData, options =>
            {
                options.SkipRule<KoordinatreferansesystemForKart2D>();
                options.SkipRule<KoordinatreferansesystemForKart3D>();
                skipRules.ForEach(options.SkipRule);
                options.OnRuleExecuted = OnRuleExecuted;
            });

            gmlValidationData.Dispose();
            genericGmlValidationData.Dispose();

            await _notificationService.SendAsync("Lager rapport");

            return _validator.GetAllRules();
        }

        private async Task OnRuleExecuted(RuleResult result)
        {
            await _notificationService.SendAsync($"{result} ({result.TimeUsed:0.##} sek.)");
        }

        private static async Task<IGmlValidationData> GetGmlValidationData(DisposableList<InputData> inputData)
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

            return GmlValidationData.Create(
                gmlDocuments2D,
                gmlDocuments3D
            );
        }
    }
}
