using System;

namespace Geonorge.Validator.Application.HttpClients.GmlApplicationSchemaRegistry
{
    public class GmlApplicationSchemaRegistrySettings
    {
        public static readonly string SectionName = "GmlApplicationSchemaRegistry";
        public Uri RegisterUri { get; set; }
        public string CacheFilePath { get; set; }
        public int CacheDurationDays { get; set; }
    }
}
