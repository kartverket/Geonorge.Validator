using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Geonorge.XsdValidator.Validator
{
    internal sealed class XmlReaderPathWrapper : IDisposable
    {
        private const string PathSeparator = "/";

        private readonly Stack<string> _previousNames = new();
        private readonly XmlReader _reader;

        private string Name => $"*:{_reader.LocalName}";
        private int Depth => _reader.Depth;
        public string Path => $"{string.Join(PathSeparator, _previousNames.Reverse())}/{Name}";

        public XmlReaderPathWrapper(XmlReader reader)
        {
            _reader = reader;
        }

        public bool Read()
        {
            var lastDepth = Depth;
            var lastName = Name;

            if (!_reader.Read())
                return false;

            if (Depth > lastDepth)
            {
                _previousNames.Push(lastName);
            }
            else if (Depth < lastDepth)
            {
                _previousNames.Pop();
            }

            return true;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
