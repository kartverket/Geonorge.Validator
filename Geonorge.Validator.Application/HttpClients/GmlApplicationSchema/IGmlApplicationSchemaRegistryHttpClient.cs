using Geonorge.Validator.Application.Models.Geonorge;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.GmlApplicationSchemaRegistry
{
    public interface IGmlApplicationSchemaRegistryHttpClient
    {
        Task<List<ApplicationSchema>> GetGmlApplicationSchemaRegistryAsync();
        Task<List<ApplicationSchema>> CreateGmlApplicationSchemaRegistryAsync();
    }
}
