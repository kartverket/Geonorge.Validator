using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Wmhelp.XPath2;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class CodelistItem
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Description { get; private set; }

        public CodelistItem(string name, string value, string description)
        {
            Name = name;
            Value = value;
            Description = description;
        }
    }
}
