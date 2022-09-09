using DiBK.RuleValidator.Config;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.JsonSchema;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Hubs;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Cache;
using Geonorge.Validator.Application.Services.JsonSchemaValidation;
using Geonorge.Validator.Application.Services.JsonValidation;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.RuleSetService;
using Geonorge.Validator.Application.Services.XmlValidation;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Validators.GenericGml;
using Geonorge.Validator.Application.Validators.GenericJson;
using Geonorge.Validator.Rules.GeoJson;
using Geonorge.Validator.Web;
using Geonorge.Validator.Web.Configuration;
using Geonorge.Validator.Web.Middleware;
using Geonorge.XsdValidator.Config;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using OSGeo.OGR;
using Reguleringsplanforslag.Rules;
using Serilog;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DiBK.RuleValidator.Extensions.Gml.Constants.Namespace;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = new[] { "application/json" };
    options.Providers.Add<GzipCompressionProvider>();
});

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

services.AddSignalR();

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Geonorge Validator", Version = "v1" });
    options.OperationFilter<MultipartOperationFilter>();
});

services.AddRuleValidator(settings =>
{
    settings.AddRuleAssembly("Geonorge.Validator.Application");
    settings.AddRuleAssembly("Geonorge.Validator.Rules.GeoJson");
    settings.AddRuleAssembly("DiBK.RuleValidator.Rules.Gml");
    settings.AddRuleAssembly("Reguleringsplanforslag.Rules");

    settings.MaxMessageCount = 500;
});

services.AddRuleValidators();

services.ConfigureRuleInformation(options =>
{
    options.AddRuleInformation<IGenericGmlValidationData>("Generell GML");

    options.AddRuleInformation<IGmlValidationData>("Generell geometri", options =>
    {
        options.SkipRule<KoordinatreferansesystemForKart2D>();
        options.SkipRule<KoordinatreferansesystemForKart3D>();
    });
    
    options.AddRuleInformation<IRpfValidationData>("Reguleringsplanforslag 5.0", options =>
    {
        options.SkipGroup("PlankartOgPlanbestemmelser");
        options.SkipGroup("Planbestemmelser");
        options.SkipGroup("Oversendelse");
    });

    options.AddRuleInformation<IGeoJsonValidationInput>("Generell GeoJSON");
});

services.AddXsdValidator(configuration, options =>
{
    options.AddCodelistSelector(
        GmlNs, 
        "CodeType", 
        element => element.Descendants(GmlNs + "defaultCodeSpace").SingleOrDefault()?.Value
    );
});

services.AddHttpContextAccessor();

services.AddTransient<IGenericGmlValidator, GenericGmlValidator>();
services.AddTransient<IGenericGeoJsonValidator, GenericGeoJsonValidator>();

services.AddTransient<IXmlSchemaValidationService, XmlSchemaValidationService>();
services.AddTransient<IJsonSchemaValidationService, JsonSchemaValidationService>();
services.AddTransient<IXmlValidationService, XmlValidationService>();
services.AddTransient<IJsonValidationService, JsonValidationService>();
services.AddTransient<INotificationService, NotificationService>();
services.AddTransient<IRuleSetService, RuleSetService>();
services.AddTransient<IMultipartRequestService, MultipartRequestService>();

services.AddHttpClient<IXmlSchemaHttpClient, XmlSchemaHttpClient>();
services.AddHttpClient<IJsonSchemaHttpClient, JsonSchemaHttpClient>();
services.AddHttpClient<ICodelistHttpClient, CodelistHttpClient>();

services.AddHostedService<CacheService>();

services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
services.Configure<CodelistSettings>(configuration.GetSection(CodelistSettings.SectionName));
services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});

var urlProxy = configuration.GetValue<string>("UrlProxy");

if (!string.IsNullOrWhiteSpace(urlProxy))
{
    var proxy = new WebProxy(urlProxy) { Credentials = CredentialCache.DefaultCredentials };
    WebRequest.DefaultWebProxy = proxy;
    HttpClient.DefaultProxy = proxy;
}

var cultureInfo = new CultureInfo("nb-NO");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

Ogr.RegisterAll();
Ogr.UseExceptions();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

builder.Logging.AddSerilog(Log.Logger, true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Geonorge Validator v1");
});

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowCredentials()
    .WithMethods("GET", "POST")
    .WithOrigins("http://localhost:3000"));

app.UseResponseCompression();

app.UseMiddleware<SerilogMiddleware>();

app.Use(async (context, next) => {
    context.Request.EnableBuffering();
    await next();
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.MapHub<NotificationHub>("/hubs/notification");

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();