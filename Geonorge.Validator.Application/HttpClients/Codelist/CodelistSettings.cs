using System;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistSettings
    {
        public static readonly string SectionName = "Codelists";
        public string CacheFilesPath { get; set; }
        public string CachedUrisFileName { get; set; }
        public string[] AllowedHosts { get; set; }
        public StaticSettings Static { get; set; } = new();

        public class StaticSettings
        {
            public Uri Arealformål { get; set; }
            public Uri Feltnavn { get; set; }
            public Uri Hensynskategori { get; set; }
            public Uri Målemetode { get; set; }
        }
    }
}
