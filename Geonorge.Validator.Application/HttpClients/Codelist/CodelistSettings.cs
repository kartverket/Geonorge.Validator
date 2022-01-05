namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistSettings
    {
        public static readonly string SectionName = "Codelists";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] AllowedHosts { get; set; }
    }
}
