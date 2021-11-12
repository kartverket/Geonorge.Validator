using System.Collections.Generic;
using System.IO;

namespace Geonorge.XsdValidator.Validator
{
    public interface IXsdValidator
    {
        List<string> Validate(Stream xmlStream, Stream xsdStream);
    }
}
