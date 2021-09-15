using Arkitektum.XmlSchemaValidator.Config;
using DiBK.RuleValidator.Config;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.Rules.Schema.Planomriss;
using Geonorge.Validator.Application.Rules.Schema.Reguleringsplanforslag;
using Geonorge.Validator.Application.Services;
using Geonorge.Validator.Application.Services.Validators;
using Geonorge.Validator.Web.Configuration;
using Geonorge.Validator.Web.Middleware;
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
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Geonorge.Validator
{
    public class Startup
    {
        private static readonly Assembly _schemaRuleAssembly = Assembly.Load("Geonorge.Validator.Application");

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
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Geonorge Validator", Version = "v1" });
            });

            services.AddRuleValidator(new[] 
            {
                _schemaRuleAssembly,
                Assembly.Load("DiBK.RuleValidator.Rules.Gml")
            });

            services.AddValidators(options =>
            {
                options.AddValidator<IReguleringsplanforslagValidator, ReguleringsplanforslagValidator>(
                    ValidatorType.Reguleringsplanforslag,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0",
                    new[] { typeof(SkjemavalideringForReguleringsplanforslag), typeof(IGmlValidationData) },
                    new[] { ".gml" }
                );

                options.AddValidator<IPlanomrissValidator, PlanomrissValidator>(
                    ValidatorType.Planomriss,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0/Planomriss",
                    new[] { typeof(SkjemavalideringForGmlPlanomriss), typeof(IGmlValidationData) },
                    new[] { ".gml" }
                );
            });

            services.AddXmlSchemaValidator(options =>
            {
                options.AddSchema(
                    ValidatorType.Reguleringsplanforslag,
                    "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Reguleringsplanforslag/5.0",
                    "http://skjema.geonorge.no/SOSITEST/produktspesifikasjon/Reguleringsplanforslag/5.0/reguleringsplanforslag-5.0_rev20210827.xsd"
                );

                options.AddSchema(
                    ValidatorType.Planomriss,
                    GetResourceStream("planomriss-5.0.xsd")                    
                );

                options.CacheFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Geonorge.Validator/XSD");
                options.CacheDurationDays = 30;
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IValidationService, ValidationService>();
            services.AddTransient<IRuleService, RuleService>();
            services.AddTransient<IXsdValidationService, XsdValidationService>();
            services.AddTransient<IValidatorService, ValidatorService>();
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
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Geonorge Validator v1"));

            app.UseCors(options => options
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin());

            app.UseMiddleware<SerilogMiddleware>();

            app.UseXmlSchemaValidator();

            app.UseHttpsRedirection();

            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            hostApplicationLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
        }

        private static Stream GetResourceStream(string fileName)
        {
            var name = _schemaRuleAssembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith(fileName));

            return name != null ? _schemaRuleAssembly.GetManifestResourceStream(name) : null;
        }
    }
}
