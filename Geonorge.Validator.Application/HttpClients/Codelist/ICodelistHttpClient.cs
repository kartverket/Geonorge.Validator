using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.Codelist;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeList = Geonorge.Validator.Application.Models.Data.Codelist.Codelist;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public interface ICodelistHttpClient
    {
        Task<List<CodeSpace>> GetCodeSpacesAsync(Dictionary<string, Uri> codelistUris);
        Task<List<GmlCodeSpace>> GetGmlCodeSpacesAsync(Dictionary<string, Uri> codelistUris);
        Task<CodeList> GetCodelistAsync(Uri uri);
        Task<int> UpdateCacheAsync();
    }
}
