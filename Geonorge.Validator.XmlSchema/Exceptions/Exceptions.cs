using System;

namespace Geonorge.Validator.XmlSchema.Exceptions
{
    public class XmlSchemaValidationException : Exception
    {
        public XmlSchemaValidationException()
        {
        }

        public XmlSchemaValidationException(string message) : base(message)
        {
        }

        public XmlSchemaValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
