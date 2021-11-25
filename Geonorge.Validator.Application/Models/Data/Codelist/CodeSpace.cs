using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class CodeSpace
    {
        public string XPath { get; private set; }
        public string Url { get; private set; }
        public List<CodelistItem> Codelist { get; private set; }

        public CodeSpace(string xPath, string url, List<CodelistItem> codelist)
        {
            XPath = xPath;
            Url = url;
            Codelist = codelist;
        }
    }
}
