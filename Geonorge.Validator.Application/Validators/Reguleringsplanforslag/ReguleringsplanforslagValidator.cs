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
using GmlHelper = Geonorge.Validator.Application.Utils.GmlHelper;

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

        public async Task<List<Rule>> Validate(string xmlNamespace, DisposableList<InputData> inputData)
        {
            await _notificationService.SendAsync("Bearbeider data");

            var gmlValidationData = await GetGmlValidationData(inputData);
            var rpfValidationData = RpfValidationData.Create(gmlValidationData.Surfaces, gmlValidationData.Solids.FirstOrDefault(), await GetKodelister());

            var options = _options.GetValidationOptions(xmlNamespace);

            await _notificationService.SendAsync("Validerer");

            await _validator.Validate(gmlValidationData);
            await _validator.Validate(rpfValidationData, options);

            gmlValidationData.Dispose();
            rpfValidationData.Dispose();

            await _notificationService.SendAsync("Lager rapport");

            return _validator.GetAllRules();
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
                var dimensions = await GmlHelper.GetDimensionsAsync(data.Stream);

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