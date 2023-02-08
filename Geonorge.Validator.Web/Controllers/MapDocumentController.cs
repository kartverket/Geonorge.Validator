using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Map.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Geonorge.Validator.Map.Constants.Constants;

namespace Geonorge.Validator.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapDocumentController : BaseController
    {
        private readonly IMapDocumentService _mapDocumentService;
        private readonly IMultipartRequestService _multipartRequestService;

        public MapDocumentController(
            IMapDocumentService mapDocumentService,
            IMultipartRequestService multipartRequestService,
            ILogger<MapDocumentController> logger) : base(logger)
        {
            _mapDocumentService = mapDocumentService;
            _multipartRequestService = multipartRequestService;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)]
        [RequestSizeLimit(104_857_600)]
        public async Task<IActionResult> CreateMapDocument()
        {
            try
            {
                var formFile = await _multipartRequestService.GetGmlFileFromMultipartAsync();

                if (formFile == null)
                    return BadRequest("Filen er ikke en gyldig GML-fil!");

                var document = await _mapDocumentService.CreateMapDocumentAsync(formFile);
                var serialized = JsonConvert.SerializeObject(document, DefaultJsonSerializerSettings);

                return Ok(serialized);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }
    }
}
