using DiBK.RuleValidator.Extensions.Gml;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class GmlValidationInputV2 : IGmlValidationInputV2
    {
        private bool _disposed = false;
        public List<GmlDocument> Documents { get; } = new();
        public XLinkValidator XLinkValidator { get; }

        private GmlValidationInputV2(
            IEnumerable<GmlDocument> documents, 
            XLinkValidator xLinkValidator)
        {
            Documents.AddRange(documents ?? new List<GmlDocument>());
            XLinkValidator = xLinkValidator;
        }

        public static IGmlValidationInputV2 Create(
            IEnumerable<GmlDocument> documents, 
            XLinkValidator xLinkValidator)
        {
            return new GmlValidationInputV2(documents, xLinkValidator);
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
                    Documents.ForEach(surface => surface.Dispose());

                _disposed = true;
            }
        }
    }
}
