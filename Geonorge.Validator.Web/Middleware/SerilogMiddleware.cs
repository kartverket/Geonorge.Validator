using Geonorge.Validator.Application.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Geonorge.Validator.Web.Middleware
{
    public class SerilogMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            using (ContextCorrelator.BeginCorrelationScope("CorrelationId", Guid.NewGuid().ToString()))
            {
                await _next(context);
            }
        }
    }
}
