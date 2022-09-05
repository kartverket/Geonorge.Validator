﻿using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Services.RuleSetService;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Controllers
{
    [ApiController]
    [Route("regler")]
    public class RuleSetController : BaseController
    {
        private readonly IRuleSetService _ruleSetService;

        public RuleSetController(
            IRuleSetService ruleSetService,
            ILogger<RuleSetController> logger) : base(logger)
        {
            _ruleSetService = ruleSetService;
        }

        [HttpGet]
        public IActionResult GetRuleSets()
        {
            try
            {
                var ruleSets = _ruleSetService.GetRuleSets();

                return Ok(ruleSets);
            }
            catch (Exception exception)
            {
                var result = HandleException(exception);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_048_576_000)]
        [RequestSizeLimit(1_048_576_000)]
        public async Task<IActionResult> GetRuleSetsForNamespace()
        {
            try
            {
                var ruleSets = await _ruleSetService.GetRuleSetsForNamespace();

                if (ruleSets == null)
                    return BadRequest();

                return Ok(ruleSets);
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
