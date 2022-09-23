using Geonorge.Validator.XmlSchema.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Geonorge.Validator.XmlSchema.Config
{
    public static class XmlSchemaValidatorConfig
    {
        public static void AddXmlSchemaValidator(this IServiceCollection services, IConfiguration configuration, Action<XmlSchemaValidatorSettings> options)
        {
            var settings = new XmlSchemaValidatorSettings();
            options.Invoke(settings);
            configuration.GetSection(XmlSchemaValidatorSettings.SectionName).Bind(settings);

            services.AddSingleton(Options.Create(settings));
            services.AddTransient<IXmlSchemaValidator, XmlSchemaValidator>();
        }
    }
}
