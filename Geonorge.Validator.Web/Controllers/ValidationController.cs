﻿using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Services.JsonValidation;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Services.XmlValidation;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("validering")]
    public class ValidationController : BaseController
    {
        private readonly IXmlValidationService _xmlValidationService;
        private readonly IJsonValidationService _jsonValidationService;
        private readonly IMultipartRequestService _multipartRequestService;

        public ValidationController(
            IMultipartRequestService multipartRequestService,
            IXmlValidationService validationService,
            IJsonValidationService jsonValidationService,
            ILogger<ValidationController> logger) : base(logger)
        {
            _multipartRequestService = multipartRequestService;
            _xmlValidationService = validationService;
            _jsonValidationService = jsonValidationService;
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_048_576_000)]
        [RequestSizeLimit(1_048_576_000)]
        public async Task<IActionResult> Validate()
        {
            try
            {
                var submittal = await _multipartRequestService.GetFilesFromMultipart();
                
                if (!submittal.IsValid)
                    return BadRequest();

                ValidationReport report = null;

                switch (submittal.FileType)
                {
                    case FileType.XML or FileType.GML32:
                        report = await _xmlValidationService.ValidateAsync(submittal);
                        break;
                    case FileType.JSON:
                        report = await _jsonValidationService.ValidateAsync(submittal);
                        break;
                    default:
                        return BadRequest();
                }

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
