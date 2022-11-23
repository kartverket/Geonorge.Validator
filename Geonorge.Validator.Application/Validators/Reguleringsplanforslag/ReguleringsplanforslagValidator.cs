using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.Extensions.Options;
using Reguleringsplanforslag.Rules.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Common.Helpers.ValidationHelper;
using static Geonorge.Validator.Common.Helpers.GmlHelper;

namespace Geonorge.Validator.Application.Validators.Reguleringsplanforslag
{
    public class ReguleringsplanforslagValidator : IReguleringsplanforslagValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly INotificationService _notificationService;
        private readonly ValidatorOptions _options;
        private readonly CodelistSettings _codelistSettings;

        public ReguleringsplanforslagValidator(
            IRuleValidator validator,
            ICodelistHttpClient codelistHttpClient,
            INotificationService notificationService,
            IOptions<ValidatorOptions> options,
            IOptions<CodelistSettings> codelistOptions)
        {
            _validator = validator;
            _codelistHttpClient = codelistHttpClient;
            _notificationService = notificationService;
            _options = options.Value;
            _codelistSettings = codelistOptions.Value;
        }

        public async Task<List<Rule>> Validate(string xmlNamespace, DisposableList<InputData> inputData, List<string> skipRules)
        {
            await _notificationService.SendAsync("Bearbeider data");

            var gmlValidationInputV1 = await GetGmlValidationInputV1(inputData);
            var rpfValidationInput = RpfValidationInput.Create(gmlValidationInputV1.Surfaces, gmlValidationInputV1.Solids.FirstOrDefault(), await GetKodelister());

            var optionsAction = _options.GetValidationOptions(xmlNamespace);
            var options = CreateValidationOptions(optionsAction, skipRules);
            options.OnRuleExecuted = OnRuleExecuted;

            await _notificationService.SendAsync("Validerer");

            await _validator.Validate(gmlValidationInputV1, options);
            await _validator.Validate(rpfValidationInput, options);

            gmlValidationInputV1.Dispose();
            rpfValidationInput.Dispose();

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

        private async Task<Kodelister> GetKodelister()
        {
            return new Kodelister
            {
                Arealformål = await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Arealformål),
                Feltnavn = await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Feltnavn),
                Hensynskategori = await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Hensynskategori)
            };
        }
    }
}