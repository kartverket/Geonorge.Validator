using System;
using System.Net;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class CodelistDocument
    {
        public CodelistDocument(Uri uri, XDocument document, HttpStatusCode statusCode)
        {
            Uri = uri;
            Document = document;
            StatusCode = statusCode;
        }

        public Uri Uri { get; }
        public XDocument Document { get; }
        public HttpStatusCode StatusCode { get; }
    }
}
