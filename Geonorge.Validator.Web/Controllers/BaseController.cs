using Geonorge.Validator.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Geonorge.Validator.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<ControllerBase> _logger;

        protected BaseController(
            ILogger<ControllerBase> logger)
        {
            _logger = logger;
        }

        protected IActionResult HandleException(Exception exception)
        {
            _logger.LogError(exception.ToString());

            return exception switch
            {
                ArgumentException _ or InvalidDataException _ or FormatException _ => BadRequest(),
                InvalidFileException ex => BadRequest(ex.Message),
                Exception _ => StatusCode(StatusCodes.Status500InternalServerError),
                _ => null,
            };
        }
    }
}
