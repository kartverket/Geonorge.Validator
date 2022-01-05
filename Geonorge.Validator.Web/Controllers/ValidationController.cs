using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Services.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("validering")]
    public class ValidationController : BaseController
    {
        private readonly IValidationService _validationService;
        private readonly IMultipartRequestService _multipartRequestService;

        public ValidationController(
            IValidationService validationService,
            IMultipartRequestService multipartRequestService,
            ILogger<ValidationController> logger) : base(logger)
        {
            _validationService = validationService;
            _multipartRequestService = multipartRequestService;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209_715_200)]
        [RequestSizeLimit(209_715_200)]
        public async Task<IActionResult> Validate()
        {
            try
            {
                var inputFiles = await _multipartRequestService.GetFilesFromMultipart();

                if (!inputFiles.XmlFiles.Any())
                    return BadRequest();

                var report = await _validationService.ValidateAsync(inputFiles.XmlFiles, inputFiles.XsdFile);

                return Ok(report);
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
