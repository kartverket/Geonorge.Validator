namespace Geonorge.Validator.Common.Models
{
    public class XmlLineInfo
    {
        public XmlLineInfo(int lineNumber, int linePosition)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

        public int LineNumber { get; }
        public int LinePosition { get; }

        public override bool Equals(object obj)
        {
            return obj is XmlLineInfo info &&
                   LineNumber == info.LineNumber &&
                   LinePosition == info.LinePosition;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LineNumber, LinePosition);
        }
    }
}
