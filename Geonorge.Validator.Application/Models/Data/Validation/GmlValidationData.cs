using DiBK.RuleValidator.Extensions;
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
        public List<CodelistItem> Målemetoder { get; } = new();

        private GmlValidationData(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids, IEnumerable<CodelistItem> målemetoder)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
            Målemetoder.AddRange(målemetoder ?? new List<CodelistItem>());
        }

        public static IGmlValidationData Create(IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids, IEnumerable<CodelistItem> målemetoder)
        {
            return new GmlValidationData(surfaces, solids, målemetoder);
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
