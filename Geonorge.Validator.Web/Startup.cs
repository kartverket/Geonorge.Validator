using DiBK.RuleValidator.Config;
using Geonorge.Validator.Application.HttpClients.StaticData;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Services.RuleService;
using Geonorge.Validator.Application.Services.Validation;
using Geonorge.Validator.Application.Services.Validator;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Web.Configuration;
using Geonorge.Validator.Web.Middleware;
using Geonorge.XsdValidator.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OSGeo.OGR;
using Serilog;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Geonorge.Validator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Geonorge Validator", Version = "v1" });
            });

            services.AddRuleValidator(new[] 
            {
                Assembly.Load("Geonorge.Validator.Application"),
                Assembly.Load("DiBK.RuleValidator.Rules.Gml"),
                Assembly.Load("Reguleringsplanforslag.Rules"),
            });

            services.AddRuleValidators();

            services.AddXsdValidator(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IValidationService, ValidationService>();
            services.AddTransient<IRuleService, RuleService>();
            services.AddTransient<IXsdValidationService, XsdValidationService>();
            services.AddTransient<IValidatorService, ValidatorService>();

            services.AddHttpClient<IXsdHttpClient, XsdHttpClient>();
            services.AddHttpClient<IStaticDataHttpClient, StaticDataHttpClient>();

            services.Configure<StaticDataSettings>(Configuration.GetSection(StaticDataSettings.SectionName));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IHostApplicationLifetime hostApplicationLifetime)
        {
            var cultureInfo = new CultureInfo("nb-NO");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            Ogr.RegisterAll();
            Ogr.UseExceptions();

            loggerFactory.AddSerilog(Log.Logger, true);

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Geonorge Validator v1");
                options.RoutePrefix = "api/swagger";
            });

            app.UseCors(options => options
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());

            app.UseMiddleware<SerilogMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            hostApplicationLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }
    }
}
