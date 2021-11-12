using Geonorge.Validator.Application.Services.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("validering")]
    public class ValidationController : BaseController
    {
        private readonly IValidationService _validationService; 

        public ValidationController(
            IValidationService validationService,
            ILogger<ValidationController> logger) : base(logger)
        {
            _validationService = validationService;
        }

        [HttpPost]
        public async Task<IActionResult> Validate(List<IFormFile> xmlFiles, IFormFile xsdFile = null)
        {
            try
            {
                if (!xmlFiles?.Any() ?? true)
                    return BadRequest();

                var report = await _validationService.Validate(xmlFiles, xsdFile);

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
