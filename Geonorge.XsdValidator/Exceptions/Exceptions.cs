using System;

namespace Geonorge.XsdValidator.Exceptions
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
