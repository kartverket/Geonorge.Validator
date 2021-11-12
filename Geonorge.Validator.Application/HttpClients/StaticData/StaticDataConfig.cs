namespace Geonorge.Validator.Application.HttpClients.StaticData
{
    public class StaticDataConfig
    {
        public static readonly string SectionName = "StaticDataConfig";
        public DataSource Arealformål { get; set; }
        public DataSource FeltnavnArealformål { get; set; }
        public DataSource Hensynskategori { get; set; }

        public class DataSource
        {
            public string Url { get; set; }
            public string FileName { get; set; }
            public int CacheDays { get; set; }
        }
    }
}
