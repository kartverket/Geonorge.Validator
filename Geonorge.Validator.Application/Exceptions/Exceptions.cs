using System;

namespace Geonorge.Validator.Application.Exceptions
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

    public class InvalidXsdException : Exception
    {
        public InvalidXsdException()
        {
        }

        public InvalidXsdException(string message) : base(message)
        {
        }

        public InvalidXsdException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
