using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Models.Data.Validation;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Wmhelp.XPath2;
using static Geonorge.Validator.Application.Utils.ValidationHelpers;

namespace Geonorge.Validator.Application.Validators.GenericGml
{
    public class GenericGmlValidator : IGenericGmlValidator
    {
        private static readonly string _gml32Namespace = "http://www.opengis.net/gml/3.2";
        private readonly IRuleValidator _validator;
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly IXsdCodelistExtractor _xsdPathExtractor;

        public GenericGmlValidator(
            IRuleValidator validator,
            ICodelistHttpClient codelistHttpClient,
            IXsdCodelistExtractor xsdPathExtractor)
        {
            _validator = validator;
            _codelistHttpClient = codelistHttpClient;
            _xsdPathExtractor = xsdPathExtractor;
        }

        public async Task<List<Rule>> Validate(DisposableList<InputData> inputData, Stream xsdStream)
        {
            var codeSpaces = await GetCodeSpaces(inputData, xsdStream);

            using var gmlValidationData = GetGmlValidationData(inputData);

            using var genericGmlValidationData = GenericGmlValidationData.Create(
                gmlValidationData.Surfaces, 
                gmlValidationData.Solids,
                null
                //await _codelistHttpClient.GetCodeSpaces(xsdStream)
            );

            _validator.Validate(gmlValidationData, options => options.SkipRule<Datasettoppløsning>());
            _validator.Validate(genericGmlValidationData);

            return _validator.GetAllRules();
        }

        private async Task<List<CodeSpace>> GetCodeSpaces(DisposableList<InputData> inputData, Stream xsdStream)
        {
            return await _codelistHttpClient.GetCodeSpaces(
                inputData.Where(data => data.IsValid).Select(data => data.Stream),
                xsdStream,
                new()
                {
                    new CodelistSelector(
                        new XmlQualifiedName("CodeType", "http://www.opengis.net/gml/3.2"),
                        element => element.XPath2SelectElement("//*:defaultCodeSpace")?.Value
                    )
                }
            );
        }

        private static IGmlValidationData GetGmlValidationData(DisposableList<InputData> inputData)
        {
            var gmlDocuments2D = new List<GmlDocument>();
            var gmlDocuments3D = new List<GmlDocument>();

            foreach (var data in inputData)
            {
                if (!data.IsValid)
                    continue;

                (string gmlNamespace, int dimensions) = GetGmlMetadata(data);

                if (!IsGml32(gmlNamespace))
                    continue;

                var document = GmlDocument.Create(data);

                if (dimensions == 2)
                    gmlDocuments2D.Add(document);
                else if (dimensions == 3)
                    gmlDocuments3D.Add(document);
            }

            return GmlValidationData.Create(gmlDocuments2D, gmlDocuments3D);
        }

        private static bool IsGml32(string gmlNamespace) => gmlNamespace == _gml32Namespace;
    }
}
