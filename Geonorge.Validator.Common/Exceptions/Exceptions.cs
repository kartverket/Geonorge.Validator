namespace Geonorge.Validator.Common.Exceptions
{
    public class InvalidFileException : Exception
    {
        public InvalidFileException()
        {
        }

        public InvalidFileException(string message) : base(message)
        {
        }

        public InvalidFileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

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

    public class InvalidJsonSchemaException : Exception
    {
        public InvalidJsonSchemaException()
        {
        }

        public InvalidJsonSchemaException(string message) : base(message)
        {
        }

        public InvalidJsonSchemaException(string message, Exception innerException) : base(message, innerException)
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
