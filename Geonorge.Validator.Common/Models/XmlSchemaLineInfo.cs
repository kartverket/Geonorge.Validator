namespace Geonorge.Validator.Common.Models
{
    public class XmlSchemaLineInfo
    {
        public XmlSchemaLineInfo(int lineNumber, int linePosition, string sourceUri)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
            SourceUri = sourceUri;
        }

        public int LineNumber { get; }
        public int LinePosition { get; }
        public string SourceUri { get; }

        public override bool Equals(object obj)
        {
            return obj is XmlSchemaLineInfo info &&
                   LineNumber == info.LineNumber &&
                   LinePosition == info.LinePosition &&
                   SourceUri == info.SourceUri;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LineNumber, LinePosition, SourceUri);
        }
    }
}
