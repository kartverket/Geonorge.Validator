using DiBK.RuleValidator.Config;
using DiBK.RuleValidator.Rules.Gml;
using Geonorge.Validator.Application.HttpClients.Codelist;
using Geonorge.Validator.Application.HttpClients.Xsd;
using Geonorge.Validator.Application.Hubs;
using Geonorge.Validator.Application.Models.Data.Validation;
using Geonorge.Validator.Application.Services.Cache;
using Geonorge.Validator.Application.Services.MultipartRequest;
using Geonorge.Validator.Application.Services.Notification;
using Geonorge.Validator.Application.Services.RuleInfoService;
using Geonorge.Validator.Application.Services.Validation;
using Geonorge.Validator.Application.Services.XsdValidation;
using Geonorge.Validator.Application.Validators.GenericGml;
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
    settings.AddRuleAssembly("DiBK.RuleValidator.Rules.Gml");
    settings.AddRuleAssembly("Reguleringsplanforslag.Rules");

    settings.MaxMessageCount = 500;
});

services.AddRuleValidators();

services.ConfigureRuleInformation(options =>
{
    options.AddRuleInformation<IGenericGmlValidationData>("Generell GML");
    options.AddRuleInformation<IGmlValidationData>("Generell geometri");
    options.AddRuleInformation<IRpfValidationData>("Reguleringsplanforslag 5.0", options =>
    {
        options.SkipGroup("PlankartOgPlanbestemmelser");
        options.SkipGroup("Planbestemmelser");
        options.SkipGroup("Oversendelse");
    });
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

services.AddTransient<IValidationService, ValidationService>();
services.AddTransient<IXsdValidationService, XsdValidationService>();
services.AddTransient<IGenericGmlValidator, GenericGmlValidator>();
services.AddTransient<IMultipartRequestService, MultipartRequestService>();
services.AddTransient<INotificationService, NotificationService>();
services.AddTransient<IRuleInfoService, RuleInfoService>();
services.AddHttpClient<IXsdHttpClient, XsdHttpClient>();
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