using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.HttpClients.StaticData;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.Extensions.Options;
using Reguleringsplanforslag.Rules.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

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
            var gmlDocuments = GetValidationData(inputData, data => data
                .Select(data => GmlDocument.Create(data))
                .ToLookup(document => GmlHelper.GetDimensions(document.Document.Root))
            );

            using var gmlValidationData = GmlValidationData.Create(gmlDocuments[2], gmlDocuments[3]);
            using var rpfValidationData = RpfValidationData.Create(gmlDocuments[2], gmlDocuments[3].FirstOrDefault(), await GetKodelister());

            var options = _options.GetValidationOptions(xmlNamespace);

            _validator.Validate(gmlValidationData);
            _validator.Validate(rpfValidationData, options);

            return _validator.GetAllRules();
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
