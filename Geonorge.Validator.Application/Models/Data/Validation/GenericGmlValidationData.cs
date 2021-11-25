using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data.Codelist;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class GenericGmlValidationData : IGenericGmlValidationData
    {
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
            if (disposing)
                return;

            Surfaces.ForEach(surface => surface.Dispose());
            Solids.ForEach(solid => solid.Dispose());
        }
    }
}
