using Geonorge.Validator.GeoJson.Models;
using Geonorge.Validator.Rules.GeoJson;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.GeoJson
{
    public class GeoJsonValidationInput : IGeoJsonValidationInput
    {
        private bool _disposed = false;
        public List<GeoJsonDocument> Documents { get; } = new();

        private GeoJsonValidationInput(IEnumerable<GeoJsonDocument> documents)
        {
            Documents.AddRange(documents ?? new List<GeoJsonDocument>());
        }

        public static IGeoJsonValidationInput Create(IEnumerable<GeoJsonDocument> documents)
        {
            return new GeoJsonValidationInput(documents);
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
                    Documents.ForEach(document => document.Dispose());
                }

                _disposed = true;
            }
        }
    }
}
