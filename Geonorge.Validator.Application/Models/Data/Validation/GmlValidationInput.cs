using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models
{
    public class GmlValidationInput : IGmlValidationInputV1
    {
        private bool _disposed = false;
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();

        private GmlValidationInput(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
        }

        public static IGmlValidationInputV1 Create(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids)
        {
            return new GmlValidationInput(surfaces, solids);
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
