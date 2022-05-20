using Geonorge.XsdValidator.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using XmlSchemaValidator = Geonorge.XsdValidator.Validator.XsdValidator;

namespace Geonorge.XsdValidator.Config
{
    public static class XsdValidatorConfig
    {
        public static void AddXsdValidator(this IServiceCollection services, IConfiguration configuration, Action<XsdValidatorSettings> options)
        {
            var settings = new XsdValidatorSettings();
            options.Invoke(settings);
            configuration.GetSection(XsdValidatorSettings.SectionName).Bind(settings);

            services.AddSingleton(Options.Create(settings));
            services.AddTransient<IXsdValidator, XmlSchemaValidator>();
        }
    }
}
