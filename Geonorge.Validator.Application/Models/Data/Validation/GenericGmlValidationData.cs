using DiBK.RuleValidator.Extensions.Gml;
using Geonorge.Validator.Application.Models.Data.Codelist;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class GenericGmlValidationData : IGenericGmlValidationData
    {
        private bool _disposed = false;
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();
        public List<GmlCodeSpace> CodeSpaces { get; } = new();

        private GenericGmlValidationData(
            IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids, IEnumerable<GmlCodeSpace> codeSpaces)
        {
            Surfaces.AddRange(surfaces ?? new List<GmlDocument>());
            Solids.AddRange(solids ?? new List<GmlDocument>());
            CodeSpaces.AddRange(codeSpaces ?? new List<GmlCodeSpace>());
        }

        public static IGenericGmlValidationData Create(
            IEnumerable<GmlDocument> surfaces, IEnumerable<GmlDocument> solids, IEnumerable<GmlCodeSpace> codeSpaces)
        {
            return new GenericGmlValidationData(surfaces, solids, codeSpaces);
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
