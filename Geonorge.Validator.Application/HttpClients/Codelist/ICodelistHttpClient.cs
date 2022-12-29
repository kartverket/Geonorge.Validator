using System;
using System.Threading.Tasks;
using CodeList = Geonorge.Validator.Application.Models.Data.Codelist.Codelist;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public interface ICodelistHttpClient
    {
        Task<CodeList> GetCodelistAsync(Uri uri);
        Task<int> UpdateCacheAsync(bool forceUpdate = false);
    }
}
