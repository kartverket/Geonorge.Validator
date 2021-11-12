using Geonorge.XsdValidator.Validator;
using Microsoft.Extensions.DependencyInjection;
using System;
using XmlSchemaValidator = Geonorge.XsdValidator.Validator.XsdValidator;

namespace Geonorge.XsdValidator.Config
{
    public static class XsdValidatorConfig
    {
        public static void AddXsdValidator(this IServiceCollection services, Action<XsdValidatorOptions> options)
        {
            services.Configure(options);
            services.AddTransient<IXsdValidator, XmlSchemaValidator>();
        }
    }
}
