using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Models
{
    public class ValidationSumbittal
    {
        public List<IFormFile> Files { get; set; }
        public string Namespace { get; set; }
        public bool IsValid => (Files?.Any() ?? false) && !string.IsNullOrWhiteSpace(Namespace);
    }
}
