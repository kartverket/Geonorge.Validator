using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Models.Data
{
    public class Submittal
    {
        public Submittal(List<IFormFile> files, IFormFile schema, List<string> skipRules, FileType fileType)
        {
            Files = files;
            Schema = schema;
            SkipRules = skipRules;
            FileType = fileType;
        }

        public Submittal()
        {
        }

        public List<IFormFile> Files { get; private set; } = new();
        public IFormFile Schema { get; private set; }
        public List<string> SkipRules { get; private set; } = new();
        public FileType FileType { get; private set; } = FileType.Unknown;
        public bool IsValid => Files.Any() && FileType != FileType.Unknown;
    }
}
