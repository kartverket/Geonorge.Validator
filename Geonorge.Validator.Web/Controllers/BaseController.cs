using Geonorge.Validator.Application.Exceptions;
using Geonorge.Validator.XmlSchema.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Web.Controllers
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
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogError(exception.ToString());
#pragma warning restore CA2254 // Template should be a static expression

            return exception switch
            {
                ArgumentException or InvalidDataException or FormatException => BadRequest("Kunne ikke validere datasett"),
                InvalidFileException or InvalidXmlSchemaException or InvalidJsonSchemaException or XmlSchemaValidationException => BadRequest(exception.Message),
                Exception _ => StatusCode(500, "En ubehandlet feil har oppstått. Hvis feilen vedvarer, vennligst ta kontakt med systemadministrator."),
                _ => null,
            };
        }
    }
}
