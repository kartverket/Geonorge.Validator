using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Geonorge
{
    public class ApplicationSchema
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public List<ApplicationSchemaVersion> Versions { get; set; } = new();
    }
}
