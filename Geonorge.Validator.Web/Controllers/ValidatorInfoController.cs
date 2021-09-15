using Geonorge.Validator.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("validatorinfo")]
    public class ValidatorInfoController : BaseController
    {
        private readonly IValidatorService _validatorInfoService;


        public ValidatorInfoController(
            IValidatorService validatorInfoService,
            ILogger<ValidatorInfoController> logger) : base(logger)
        {
            _validatorInfoService = validatorInfoService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                var validatorInfo = _validatorInfoService.GetValidatorInfo();

                return Ok(validatorInfo);
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
