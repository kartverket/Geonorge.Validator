using Geonorge.Validator.Map.Services;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MapSettingsController : BaseController
    {
        private readonly IMapSettingsService _mapSettingsService;

        public MapSettingsController(
            IMapSettingsService mapSettingsService,
            ILogger<MapSettingsController> logger) : base(logger)
        {
            _mapSettingsService = mapSettingsService;
        }

        [HttpGet]
        [ResponseCache(Duration = 86400)]
        public IActionResult Get()
        {
            try
            {
                var mapSettings = _mapSettingsService.GetMapSettings();

                return Ok(mapSettings);
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
