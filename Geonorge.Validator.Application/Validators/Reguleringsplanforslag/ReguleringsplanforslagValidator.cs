using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.StaticData;
using Geonorge.Validator.Application.Models;
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
        private readonly IStaticDataHttpClient _staticDataHttpClient;
        private readonly ValidatorOptions _options;

        public ReguleringsplanforslagValidator(
            IRuleValidator validator,
            IStaticDataHttpClient staticDataHttpClient,
            IOptions<ValidatorOptions> options)
        {
            _validator = validator;
            _staticDataHttpClient = staticDataHttpClient;
            _options = options.Value;
        }

        public async Task<List<Rule>> Validate(string xmlNamespace, DisposableList<InputData> inputData)
        {
            using var gmlValidationData = await GetGmlValidationData(inputData);

            using var rpfValidationData = RpfValidationData.Create(
                gmlValidationData.Surfaces, 
                gmlValidationData.Solids.FirstOrDefault(), 
                await GetKodelister()
            );

            var options = _options.GetValidationOptions(xmlNamespace);

            _validator.Validate(gmlValidationData);
            _validator.Validate(rpfValidationData, options);

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

            return GmlValidationData.Create(gmlDocuments2D, gmlDocuments3D);
        }

        private async Task<Kodelister> GetKodelister()
        {
            return new Kodelister
            {
                Arealformål = await _staticDataHttpClient.GetArealformål(),
                Feltnavn = await _staticDataHttpClient.GetFeltnavnArealformål(),
                Hensynskategori = await _staticDataHttpClient.GetHensynskategori()
            };
        }
    }
}
