using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Utils.Codelist;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wmhelp.XPath2;
using GmlHelper = Geonorge.Validator.Application.Utils.GmlHelper;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public class GenericGmlValidator : IGenericGmlValidator
    {
        private readonly IRuleValidator _validator;
        private readonly ICodelistHttpClient _codelistHttpClient;

        public GenericGmlValidator(
            IRuleValidator validator,
            ICodelistHttpClient codelistHttpClient)
        {
            _validator = validator;
            _codelistHttpClient = codelistHttpClient;
        }

        public async Task<List<Rule>> Validate(DisposableList<InputData> inputData, Stream xsdStream, string gmlVersion)
        {
            using var gmlValidationData = await GetGmlValidationData(inputData, gmlVersion);

            using var genericGmlValidationData = GenericGmlValidationData.Create(
                gmlValidationData.Surfaces, 
                gmlValidationData.Solids,
                await GetCodeSpacesAsync(inputData, xsdStream)
            );

            _validator.Validate(gmlValidationData, options => options.SkipRule<Datasettoppløsning>());
            _validator.Validate(genericGmlValidationData);

            return _validator.GetAllRules();
        }

        private async Task<List<GmlCodeSpace>> GetCodeSpacesAsync(DisposableList<InputData> inputData, Stream xsdStream)
        {
            return await _codelistHttpClient.GetCodeSpacesForGmlAsync(
                xsdStream,
                inputData.Where(data => data.IsValid).Select(data => data.Stream),
                new[]
                {
                    new XsdCodelistSelector(
                        "CodeType", 
                        "http://www.opengis.net/gml/3.2",
                        element => element.XPath2SelectElement("//*:defaultCodeSpace")?.Value
                    )
                }
            );
        }

        private static async Task<IGmlValidationData> GetGmlValidationData(DisposableList<InputData> inputData, string gmlVersion)
        {
            var gmlDocuments2D = new List<GmlDocument>();
            var gmlDocuments3D = new List<GmlDocument>();

            foreach (var data in inputData)
            {
                if (!data.IsValid || !gmlVersion.StartsWith("3.2"))
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
