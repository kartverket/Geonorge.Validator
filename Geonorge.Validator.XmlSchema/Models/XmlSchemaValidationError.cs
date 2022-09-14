namespace Geonorge.Validator.XmlSchema.Models
{
    public class XmlSchemaValidationError
    {
        public XmlSchemaValidationError()
        {
        }

        public XmlSchemaValidationError(string message)
        {
            Message = message;
        }

        public XmlSchemaValidationError(string message, string xPath, int lineNumber, int linePosition, string fileName)
        {
            Message = message;
            XPath = xPath;
            LineNumber = lineNumber;
            LinePosition = linePosition;
            FileName = fileName;
        }

        public string Message { get; set; }
        public string XPath { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public string FileName { get; set; }
    }
}
