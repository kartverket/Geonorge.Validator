using Geonorge.Validator.Application.Services.RuleInfoService;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("regler")]
    public class RuleInfoController : BaseController
    {
        private readonly IRuleInfoService _ruleInfoService;

        public RuleInfoController(
            IRuleInfoService ruleInfoService,
            ILogger<RuleInfoController> logger) : base(logger)
        {
            _ruleInfoService = ruleInfoService;
        }

        [HttpGet]
        public IActionResult GetRuleInfo()
        {
            try
            {
                var ruleInfo = _ruleInfoService.GetRuleInfo();

                return Ok(ruleInfo);
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
