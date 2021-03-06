using Geonorge.Validator.Application.Exceptions;
using Geonorge.XsdValidator.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
                ArgumentException _ or InvalidDataException _ or FormatException _ => BadRequest("Kunne ikke validere datasett"),
                InvalidFileException _ or InvalidXsdException _ or XsdValidationException => BadRequest(exception.Message),
                Exception _ => StatusCode(StatusCodes.Status500InternalServerError),
                _ => null,
            };
        }
    }
}
