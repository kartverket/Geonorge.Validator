using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models
{
    public class GmlValidationInput : IGmlValidationInputV1
    {
        private bool _disposed = false;
        public List<GmlDocument> Documents { get; } = new();

        private GmlValidationInput(IEnumerable<GmlDocument> documents)
        {
            Documents.AddRange(documents ?? new List<GmlDocument>());
        }

        public static IGmlValidationInputV1 Create(IEnumerable<GmlDocument> documents)
        {
            return new GmlValidationInput(documents);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Documents.ForEach(document => document.Dispose());

                _disposed = true;
            }
        }
    }
}
