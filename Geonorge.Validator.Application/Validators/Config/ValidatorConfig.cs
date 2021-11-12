using Microsoft.Extensions.DependencyInjection;
using System;

namespace Geonorge.Validator.Application.Validators.Config
{
    public static class ValidatorConfig
    {
        public static void AddValidators(this IServiceCollection services, Action<ValidatorOptions> options)
        {
            services.Configure(options);

            var validatorOptions = new ValidatorOptions();
            options.Invoke(validatorOptions);

            foreach (var validator in validatorOptions.Validators)
            {
                services.AddTransient(validator.ServiceType, validator.ImplementationType);
            }
        }
    }
}
