using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.JsonSchema
{
    public interface IJsonSchemaHttpClient
    {
        Task<JSchema> GetJsonSchemaAsync(List<IFormFile> jsonFiles, IFormFile schemaFile);
    }
}
