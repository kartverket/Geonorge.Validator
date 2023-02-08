using Microsoft.AspNetCore.Http;

namespace Geonorge.Validator.Map.Models
{
    public class FormData
    {
        public IFormFile File { get; set; }
        public bool Validate { get; set; } = true;
    }
}
