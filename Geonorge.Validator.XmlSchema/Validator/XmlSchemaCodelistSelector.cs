using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Geonorge.Validator.XmlSchema.Validator
{
    public class XmlSchemaCodelistSelector
    {
        public XmlQualifiedName QualifiedName { get; private set; }
        public Func<XmlSchemaElement, Uri> UriResolver { get; private set; }

        public XmlSchemaCodelistSelector(XName elementName, Func<XmlSchemaElement, Uri> uriResolver)
        {
            QualifiedName = new(elementName.LocalName, elementName.NamespaceName);
            UriResolver = uriResolver;
        }
    }
}
