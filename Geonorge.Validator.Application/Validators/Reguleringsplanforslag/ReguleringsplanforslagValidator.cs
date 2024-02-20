using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Validators.Config;
using Innsending.Planforslag.Rules.Models;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Common.Helpers.GmlHelper;
using static Geonorge.Validator.Common.Helpers.ValidationHelper;

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

            var (Documents2D, Documents3D) = await CreateGmlDocumentsAsync(inputData);
            var gmlValidationInputV1 = GmlValidationInput.Create(Documents2D.Concat(Documents3D));
            var rpfValidationInput = RpfValidationInput.Create(Documents2D, Documents3D.FirstOrDefault(), await GetKodelister());

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

        private static async Task<(List<GmlDocument> Documents2D, List<GmlDocument> Documents3D)> CreateGmlDocumentsAsync(DisposableList<InputData> inputData)
        {
            var documents2D = new List<GmlDocument>();
            var documents3D = new List<GmlDocument>();

            foreach (var data in inputData)
            {
                if (!data.IsValid)
                    continue;

                var document = GmlDocument.Create(data);
                var dimensions = await GetDimensionsAsync(data.Stream);

                if (dimensions == 2)
                    documents2D.Add(document);
                else if (dimensions == 3)
                    documents3D.Add(document);
            }

            return (documents2D, documents3D);
        }

        private async Task<Kodelister> GetKodelister()
        {
            return new Kodelister
            {
                Arealformål = (await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Arealformål))?.Items,
                Feltnavn = (await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Feltnavn))?.Items,
                Hensynskategori = (await _codelistHttpClient.GetCodelistAsync(_codelistSettings.Static.Hensynskategori))?.Items
            };
        }
    }
}