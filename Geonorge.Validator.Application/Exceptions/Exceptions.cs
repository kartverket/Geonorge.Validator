using System;

namespace Geonorge.Validator.Application.Exceptions
{
    public class InvalidXmlSchemaException : Exception
    {
        public InvalidXmlSchemaException()
        {
        }

        public InvalidXmlSchemaException(string message) : base(message)
        {
        }

        public InvalidXmlSchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class MultipartRequestException : Exception
    {
        public MultipartRequestException()
        {
        }

        public MultipartRequestException(string message) : base(message)
        {
        }

        public MultipartRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
