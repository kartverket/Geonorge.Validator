using System;

namespace Geonorge.XsdValidator.Config
{
    public class XsdValidatorOptions
    {
        public string CacheFilesPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public int CacheDurationDays { get; set; } = 30;
        public string[] CacheableHosts { get; set; }
    }
}
