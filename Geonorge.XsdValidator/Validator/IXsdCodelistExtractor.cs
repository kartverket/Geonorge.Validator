using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.XsdValidator.Validator
{
    public interface IXsdCodelistExtractor
    {
        Task<Dictionary<string, Uri>> GetCodelistUrisAsync(Stream xsdStream, IEnumerable<Stream> xmlStreams, IEnumerable<XsdCodelistSelector> codelistSelectors);
    }
}
