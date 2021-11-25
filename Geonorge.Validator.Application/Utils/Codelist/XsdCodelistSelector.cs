using System;
using System.Xml;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.Utils.Codelist
{
    public class XsdCodelistSelector
    {
        public XmlQualifiedName QualifiedName { get; private set; }
        public Func<XElement, string> UriResolver { get; private set; }

        public XsdCodelistSelector(string elementType, string @namespace, Func<XElement, string> uriResolver)
        {
            QualifiedName = new(elementType, @namespace);
            UriResolver = uriResolver;
        }
    }
}
