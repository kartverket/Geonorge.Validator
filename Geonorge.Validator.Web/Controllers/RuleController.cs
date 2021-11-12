using Geonorge.Validator.Application.Models;
using Geonorge.Validator.Application.Services.RuleService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("regler")]
    public class RuleController : BaseController
    {
        private readonly IRuleService _ruleService; 

        public RuleController(
            IRuleService ruleService,
            ILogger<RuleController> logger) : base(logger)
        {
            _ruleService = ruleService;
        }

        [HttpPost]
        public IActionResult GetRuleInfo([FromForm] RuleSubmittal submittal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(submittal?.Namespace))
                    return BadRequest();

                var report = _ruleService.GetRuleInfo(submittal.Namespace);

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
