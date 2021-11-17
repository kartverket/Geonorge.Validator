namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistSettings
    {
        public static readonly string SectionName = "Codelists";
        public string CacheFilesPath { get; set; }
        public int CacheDurationDays { get; set; }
        public string[] AllowedHosts { get; set; }
    }
}
