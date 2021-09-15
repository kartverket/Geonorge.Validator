using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

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
        public IActionResult Validate([FromForm] ValidationSumbittal submittal)
        {
            try
            {
                if (!submittal?.IsValid ?? false)
                    return BadRequest();

                var report = _validationService.Validate(submittal.Files, submittal.Namespace);

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
