using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.GmlApplicationSchemaRegistry;
using Geonorge.Validator.Application.HttpClients.XmlSchemaCacher;
using Geonorge.Validator.XmlSchema.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Geonorge.Validator.Web.Controllers
{
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    [Route("cache")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CacheController : BaseController
    {
        private readonly ICodelistHttpClient _codelistHttpClient;
        private readonly IXmlSchemaCacherHttpClient _xmlSchemaCacherHttpClient;
        private readonly IGmlApplicationSchemaRegistryHttpClient _gmlApplicationSchemaRegistryHttpClient;
        private readonly IOptions<XmlSchemaValidatorSettings> _xmlSchemaValidatorOptions;
        private readonly IOptions<CodelistSettings> _codelistOptions;
        private readonly IOptions<GmlApplicationSchemaRegistrySettings> _gmlApplicationSchemaRegistryOptions;

        public CacheController(
            ICodelistHttpClient codelistHttpClient,
            IXmlSchemaCacherHttpClient xmlSchemaCacherHttpClient,
            IGmlApplicationSchemaRegistryHttpClient gmlApplicationSchemaRegistryHttpClient,
            IOptions<XmlSchemaValidatorSettings> xmlSchemaValidatorOptions,
            IOptions<CodelistSettings> codelistOptions,
            IOptions<GmlApplicationSchemaRegistrySettings> gmlApplicationSchemaRegistryOptions,
            ILogger<CacheController> logger) : base(logger)
        {
            _codelistHttpClient = codelistHttpClient;
            _xmlSchemaCacherHttpClient = xmlSchemaCacherHttpClient;
            _gmlApplicationSchemaRegistryHttpClient = gmlApplicationSchemaRegistryHttpClient;
            _xmlSchemaValidatorOptions = xmlSchemaValidatorOptions;
            _codelistOptions = codelistOptions;
            _gmlApplicationSchemaRegistryOptions = gmlApplicationSchemaRegistryOptions;
        }

        [HttpGet]
        [Route("clear/xsd")]
        public IActionResult ClearXsdCache()
        {
            try
            {
                var cacheFilesPath = _xmlSchemaValidatorOptions.Value.CacheFilesPath;

                if (!Directory.Exists(cacheFilesPath))
                    return NoContent();

                var directoryInfo = new DirectoryInfo(cacheFilesPath);

                foreach (var directory in directoryInfo.EnumerateDirectories())
                    directory.Delete(true);

                return NoContent();
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpGet]
        [Route("clear/codelist")]
        public IActionResult ClearCodelistCache()
        {
            try
            {
                var cacheFilesPath = _codelistOptions.Value.CacheFilesPath;

                if (!Directory.Exists(cacheFilesPath))
                    return NoContent();

                var directoryInfo = new DirectoryInfo(cacheFilesPath);

                foreach (var directory in directoryInfo.EnumerateDirectories())
                    directory.Delete(true);

                return NoContent();
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpGet]
        [Route("clear/gmlApplicationSchemas")]
        public IActionResult ClearGmlApplicationSchemasCache()
        {
            try
            {
                var cacheFilePath = _gmlApplicationSchemaRegistryOptions.Value.CacheFilePath;

                if (System.IO.File.Exists(cacheFilePath))
                    System.IO.File.Delete(cacheFilePath);

                return Ok();
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpGet]
        [Route("rebuild/xsd")]
        public async Task<IActionResult> RebuildXsdCache()
        {
            try
            {
                var count = await _xmlSchemaCacherHttpClient.UpdateCacheAsync(true);

                return Ok(count);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpGet]
        [Route("rebuild/codelist")]
        public async Task<IActionResult> RebuildCodelistCache()
        {
            try
            {
                var count = await _codelistHttpClient.UpdateCacheAsync(true);

                return Ok(count);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpGet]
        [Route("rebuild/gmlApplicationSchemas")]
        public async Task<IActionResult> RebuildGmlApplicationSchemas()
        {
            try
            {
                _ = await _gmlApplicationSchemaRegistryHttpClient.CreateGmlApplicationSchemaRegistryAsync();

                return Ok();
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
