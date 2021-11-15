using Geonorge.XsdValidator.Validator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XmlSchemaValidator = Geonorge.XsdValidator.Validator.XsdValidator;

namespace Geonorge.XsdValidator.Config
{
    public static class XsdValidatorConfig
    {
        public static void AddXsdValidator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<XsdValidatorSettings>(configuration.GetSection(XsdValidatorSettings.SectionName));
            services.AddTransient<IXsdValidator, XmlSchemaValidator>();
        }
    }
}
