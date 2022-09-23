using Geonorge.Validator.Application.HttpClients.GmlApplicationSchemaRegistry;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("gml-applikasjonsskjema")]
    public class GmlApplicationSchemaRegistryController : BaseController
    {
        private readonly IGmlApplicationSchemaRegistryHttpClient _httpClient;

        public GmlApplicationSchemaRegistryController(
            IGmlApplicationSchemaRegistryHttpClient httpClient,
            ILogger<GmlApplicationSchemaRegistryController> logger) : base(logger)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GmlApplicationSchemaRegistry()
        {
            try
            {
                var registry = await _httpClient.GetGmlApplicationSchemaRegistryAsync();

                return Ok(registry);
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
