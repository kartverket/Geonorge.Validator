using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public interface IXsdCodelistExtractor
    {
        Task<Dictionary<string, Uri>> GetCodelistsFromXsd(Stream xmlStream, Stream xsdStream, List<CodelistSelector> codelistSelectors);
    }
}
