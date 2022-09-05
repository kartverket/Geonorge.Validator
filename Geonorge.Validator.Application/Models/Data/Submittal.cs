using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data
{
    public class Submittal
    {
        public List<IFormFile> Files { get; set; } = new();
        public IFormFile Schema { get; set; }
        public List<string> SkipRules { get; set; } = new();
    }
}
