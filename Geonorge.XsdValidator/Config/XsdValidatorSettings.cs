namespace Geonorge.XsdValidator.Config
{
    public class XsdValidatorSettings
    {
        public static readonly string SectionName = "XsdValidator";
        public string CacheFilesPath { get; set; }
        public int CacheDurationDays { get; set; }
        public string[] CacheableHosts { get; set; }
    }
}
