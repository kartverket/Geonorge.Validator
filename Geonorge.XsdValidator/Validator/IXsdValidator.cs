using Geonorge.XsdValidator.Models;
using System.Collections.Generic;
using System.IO;

namespace Geonorge.XsdValidator.Validator
{
    public interface IXsdValidator
    {
        List<string> Validate(Stream xmlStream, XsdData xsdData);
    }
}
