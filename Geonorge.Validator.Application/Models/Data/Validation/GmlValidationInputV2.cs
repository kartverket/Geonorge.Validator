using DiBK.RuleValidator.Extensions.Gml;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class GmlValidationInputV2 : IGmlValidationInputV2
    {
        private bool _disposed = false;
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();
        public XLinkValidator XLinkValidator { get; }

        private GmlValidationInputV2(
            IEnumerable<GmlDocument> surfaces, 
            IEnumerable<GmlDocument> solids, 
            XLinkValidator xLinkValidator)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
            XLinkValidator = xLinkValidator;
        }

        public static IGmlValidationInputV2 Create(
            IEnumerable<GmlDocument> surfaces, 
            IEnumerable<GmlDocument> solids, 
            XLinkValidator xLinkValidator)
        {
            return new GmlValidationInputV2(surfaces, solids, xLinkValidator);
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
                {
                    Surfaces.ForEach(surface => surface.Dispose());
                    Solids.ForEach(solid => solid.Dispose());
                }

                _disposed = true;
            }
        }
    }
}
