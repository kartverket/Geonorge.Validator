using Microsoft.OpenApi;
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
                    Type = JsonSchemaType.Object,
                    Properties = new Dictionary<string, IOpenApiSchema>
                    {
                        ["files"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.String,
                                Format = "binary"
                            }
                        },
                        ["schema"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                            Format = "binary"
                        }
                    },
                    Required = new HashSet<string>() { "files" }
                }
            };
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType> { ["multipart/form-data"] = mediaType }
            };
        }
    }
}
