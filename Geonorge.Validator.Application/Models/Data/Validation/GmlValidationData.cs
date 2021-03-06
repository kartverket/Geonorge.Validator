using DiBK.RuleValidator.Extensions.Gml;
using DiBK.RuleValidator.Rules.Gml;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models
{
    public class GmlValidationData : IGmlValidationData
    {
        private bool _disposed = false;
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();

        private GmlValidationData(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
        }

        public static IGmlValidationData Create(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids)
        {
            return new GmlValidationData(surfaces, solids);
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
