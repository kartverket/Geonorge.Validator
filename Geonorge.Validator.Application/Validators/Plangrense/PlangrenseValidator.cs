using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

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
            var gmlDocuments = GetValidationData(inputData, data => data
                .Select(data => GmlDocument.Create(data))
                .ToLookup(document => GmlHelper.GetDimensions(document.Document.Root))
            );

            using var validationData = GmlValidationData.Create(gmlDocuments[2], gmlDocuments[3]);
            var options = _options.GetValidationOptions(xmlNamespace);

            _validator.Validate(validationData, options);

            return _validator.GetAllRules();
        }
    }
}
