using DiBK.RuleValidator.Extensions;
using Newtonsoft.Json.Schema;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.JsonSchema
{
    public interface IJsonSchemaHttpClient
    {
        Task<JSchema> GetJsonSchemaAsync(DisposableList<InputData> inputData, Stream schema);
        Task<int> UpdateCacheAsync(bool forceUpdate = false);
    }
}
