using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class InputFiles
    {
        public List<IFormFile> XmlFiles { get; set; } = new();
        public IFormFile XsdFile { get; set; }
    }
}
