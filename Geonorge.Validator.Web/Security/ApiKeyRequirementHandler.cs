﻿using Microsoft.AspNetCore.Authorization;

namespace Geonorge.Validator.Web.Security
{
    public class ApiKeyRequirementHandler : AuthorizationHandler<ApiKeyRequirement>
    {
        public const string API_KEY_HEADER_NAME = "X-API-KEY";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiKeyRequirementHandler(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            SucceedRequirementIfApiKeyPresentAndValid(context, requirement);

            return Task.CompletedTask;
        }

        private void SucceedRequirementIfApiKeyPresentAndValid(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            var apiKey = _httpContextAccessor.HttpContext.Request.Headers[API_KEY_HEADER_NAME].FirstOrDefault();

            if (apiKey != null && requirement.ApiKeys.Any(requiredApiKey => apiKey == requiredApiKey))
                context.Succeed(requirement);
        }
    }
}
