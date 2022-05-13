using System;

namespace Geonorge.XsdValidator.Exceptions
{
    public class XsdValidationException : Exception
    {
        public XsdValidationException()
        {
        }

        public XsdValidationException(string message) : base(message)
        {
        }

        public XsdValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
