﻿using System.Net;

namespace Geonorge.Validator.Application.Models.Data.Codelist
{
    public class CodelistResolverResult
    {
        public CodelistResolverResult(CodelistResolverStatus resolverStatus, string url, string fragment)
        {
            ResolverStatus = resolverStatus;
            Url = url;
            Fragment = fragment;
        }

        public CodelistResolverResult(CodelistResolverStatus resolverStatus, HttpStatusCode httpStatusCode, string url, string fragment)
        {
            ResolverStatus = resolverStatus;
            HttpStatusCode = httpStatusCode;
            Url = url;
            Fragment = fragment;
        }

        public CodelistResolverStatus ResolverStatus { get; }
        public HttpStatusCode? HttpStatusCode { get; }
        public string Url { get; }
        public string Fragment { get; }
    }
}