using Geonorge.Validator.XmlSchema.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using XmlSchemaValidator = Geonorge.Validator.XmlSchema.Validator.XsdValidator;

namespace Geonorge.Validator.XmlSchema.Config
{
    public static class XsdValidatorConfig
    {
        public static void AddXmlSchemaValidator(this IServiceCollection services, IConfiguration configuration, Action<XsdValidatorSettings> options)
        {
            var settings = new XsdValidatorSettings();
            options.Invoke(settings);
            configuration.GetSection(XsdValidatorSettings.SectionName).Bind(settings);

            services.AddSingleton(Options.Create(settings));
            services.AddTransient<IXsdValidator, XmlSchemaValidator>();
        }
    }
}
