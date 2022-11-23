using Geonorge.Validator.Application.Models.Data.Codelist;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.CodelistResolver
{
    public interface ICodelistResolverHttpClient
    {
        Task<CodelistResolverResult> ValidateCodelistUriAsync(string uri);
    }
}
