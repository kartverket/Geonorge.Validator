using System;
using System.Xml;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.HttpClients.Codelist
{
    public class CodelistSelector
    {
        public XmlQualifiedName QualifiedName { get; private set; }
        public Func<XElement, string> UriResolver { get; private set; }

        public CodelistSelector(XmlQualifiedName qualifiedName, Func<XElement, string> uriResolver)
        {
            QualifiedName = qualifiedName;
            UriResolver = uriResolver;
        }
    }
}
