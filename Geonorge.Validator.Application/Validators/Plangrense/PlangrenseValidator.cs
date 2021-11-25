using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using GmlHelper = Geonorge.Validator.Application.Utils.GmlHelper;

namespace Geonorge.Validator.Application.Validators.Plangrense
{
    public class PlangrenseValidator : IPlangrenseValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ValidatorOptions _options;

        public PlangrenseValidator(
            IRuleValidator validator,
            IOptions<ValidatorOptions> options)
        {
            _validator = validator;
            _options = options.Value;
        }

        public async Task<List<Rule>> Validate(string xmlNamespace, DisposableList<InputData> inputData)
        {
            using var validationData = await GetGmlValidationData(inputData);

            var options = _options.GetValidationOptions(xmlNamespace);

            _validator.Validate(validationData, options);

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
    }
}
