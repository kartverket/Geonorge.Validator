using Geonorge.Validator.Application.Models.Data.Codelist;
using Geonorge.Validator.Application.Utils.Codelist;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public interface ICodelistHttpClient
    {
        Task<List<CodeSpace>> GetCodeSpacesAsync(Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors);
        Task<List<GmlCodeSpace>> GetGmlCodeSpacesAsync(Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors);
        Task<int> UpdateCacheAsync();
    }
}
