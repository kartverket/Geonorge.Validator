using Geonorge.Validator.Application.Models.Data.Codelist;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public interface ICodelistHttpClient
    {
        Task<List<CodelistValue>> FetchCodelist(string url);
        Task<List<CodeSpace>> GetCodeSpaces(IEnumerable<Stream> xmlStreams, Stream xsdStream, List<CodelistSelector> codelistSelectors);
    }
}
