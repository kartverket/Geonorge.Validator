using Geonorge.Validator.Application.Models.Config;

namespace Geonorge.Validator.Web.Configuration
{
    public static class RuleInfoConfig
    {
        public static void ConfigureRuleInformation(this IServiceCollection services, Action<RuleInfoOptions> options)
        {
            var ruleInfoOptions = new RuleInfoOptions();
            options.Invoke(ruleInfoOptions);

            services.AddSingleton(ruleInfoOptions);
        }
    }
}
