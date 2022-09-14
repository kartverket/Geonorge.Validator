using System;
using System.Xml;
using System.Xml.Linq;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public class XsdCodelistSelector
    {
        public XmlQualifiedName QualifiedName { get; private set; }
        public Func<XElement, string> UriResolver { get; private set; }

        public XsdCodelistSelector(XNamespace @namespace, string elementType, Func<XElement, string> uriResolver)
        {
            QualifiedName = new(elementType, @namespace.NamespaceName);
            UriResolver = uriResolver;
        }
    }
}
