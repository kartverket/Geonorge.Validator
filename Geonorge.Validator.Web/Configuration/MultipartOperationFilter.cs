using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Geonorge.Validator.Web
{
    public class MultipartOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.RelativePath != "validering")
                return;

            var mediaType = new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = "object",
                    Properties =
                    {
                        ["xmlFiles"] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema
                            {
                                Type = "file",
                                Format = "binary"
                            }
                        },
                        ["xsdFile"] = new OpenApiSchema
                        {
                            Type = "file",
                            Format = "binary"
                        }
                    },
                    Required = new HashSet<string>() { "xmlFiles" }
                }
            };
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = { ["multipart/form-data"] = mediaType }
            };
        }
    }
}
