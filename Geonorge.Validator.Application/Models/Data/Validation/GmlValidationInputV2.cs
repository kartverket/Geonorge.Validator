using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.Application.Models.Data.Codelist;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class GmlValidationInputV2 : IGmlValidationInputV2
    {
        private bool _disposed = false;
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();
        public List<GmlCodeSpace> CodeSpaces { get; } = new();
        public XLinkResolver XLinkResolver { get; }

        private GmlValidationInputV2(
            IEnumerable<GmlDocument> surfaces, 
            IEnumerable<GmlDocument> solids, 
            IEnumerable<GmlCodeSpace> codeSpaces,
            XLinkResolver xLinkResolver)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
            CodeSpaces.AddRange(codeSpaces ?? new List<GmlCodeSpace>());
            XLinkResolver = xLinkResolver;
        }

        public static IGmlValidationInputV2 Create(
            IEnumerable<GmlDocument> surfaces, 
            IEnumerable<GmlDocument> solids, 
            IEnumerable<GmlCodeSpace> codeSpaces, 
            XLinkResolver xLinkResolver)
        {
            return new GmlValidationInputV2(surfaces, solids, codeSpaces, xLinkResolver);
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
