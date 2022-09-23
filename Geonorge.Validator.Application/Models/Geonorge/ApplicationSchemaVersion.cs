using System;

namespace Geonorge.Validator.Application.Models.Geonorge
{
    public class ApplicationSchemaVersion
    {
        public int VersionNumber { get; set; }
        public string VersionName { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public Uri DocumentReference { get; set; }
    }
}
