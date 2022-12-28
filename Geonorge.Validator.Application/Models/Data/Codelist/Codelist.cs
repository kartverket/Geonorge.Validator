using DiBK.RuleValidator.Extensions;
using System;
using System.Collections.Generic;
using System.Net;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class Codelist
    {
        public List<CodelistItem> Items { get; set; }
        public Uri Uri { get; set; }
        public CodelistStatus Status { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
