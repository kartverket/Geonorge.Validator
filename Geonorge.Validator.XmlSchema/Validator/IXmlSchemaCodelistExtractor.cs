using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public interface IXmlSchemaCodelistExtractor
    {
        Task<Dictionary<string, Uri>> GetCodelistUrisAsync(Stream xmlSchemaStream, IEnumerable<Stream> xmlStreams, IEnumerable<XmlSchemaCodelistSelector> codelistSelectors);
    }
}
