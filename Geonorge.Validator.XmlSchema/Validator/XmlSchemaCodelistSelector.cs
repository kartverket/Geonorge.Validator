using System;
using System.Xml;
using System.Xml.Linq;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public class XmlSchemaCodelistSelector
    {
        public XmlQualifiedName QualifiedName { get; private set; }
        public Func<XElement, string> UriResolver { get; private set; }

        public XmlSchemaCodelistSelector(XName elementName, Func<XElement, string> uriResolver)
        {
            QualifiedName = new(elementName.LocalName, elementName.NamespaceName);
            UriResolver = uriResolver;
        }
    }
}
