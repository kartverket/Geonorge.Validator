using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.GeoJson;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Validators.GenericJson
{
    public class GenericGeoJsonValidator : IGenericGeoJsonValidator
    {
        private readonly IRuleValidator _validator;
        private readonly INotificationService _notificationService;

        public GenericGeoJsonValidator(
            IRuleValidator validator,
            INotificationService notificationService)
        {
            _validator = validator;
            _notificationService = notificationService;
        }

        public async Task<List<Rule>> ValidateAsync(string schemaId, DisposableList<InputData> inputData, List<string> skipRules)
        {
            var geoJsonValidationInput = await GetGeoJsonValidationInput(inputData);

            await _validator.Validate(geoJsonValidationInput, options =>
            {
                skipRules.ForEach(options.SkipRule);
                options.OnRuleExecuted = OnRuleExecuted;
            });

            return  _validator.GetAllRules();
        }

        private async Task OnRuleExecuted(RuleResult result)
        {
            await _notificationService.SendAsync($"{result} ({result.TimeUsed:0.##} sek.)");
        }

        private static async Task<IGeoJsonValidationInput> GetGeoJsonValidationInput(DisposableList<InputData> inputData)
        {
            var geoJsonDocuments = new List<GeoJsonDocument>();

            foreach (var data in inputData)
            {
                if (!data.IsValid)
                    continue;

                geoJsonDocuments.Add(await GeoJsonDocument.CreateAsync(data));
            }

            return GeoJsonValidationInput.Create(geoJsonDocuments);
        }
    }
}
