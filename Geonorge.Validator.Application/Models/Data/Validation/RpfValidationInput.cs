using DiBK.RuleValidator.Extensions;
using DiBK.RuleValidator.Extensions.Gml;
using Reguleringsplanforslag.Rules;
using Reguleringsplanforslag.Rules.Models;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Oversendelse;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Planbestemmelser;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models.Data.Validation
{
    public class RpfValidationInput : IRpfValidationInput
    {
        private bool _disposed = false;
        public List<GmlDocument> Plankart2D { get; } = new();
        public GmlDocument Plankart3D { get; }
        public Kodelister Kodelister { get; }
        public ValidationDataElement<ReguleringsplanbestemmelserType> Planbestemmelser { get; }
        public ValidationDataElement<OversendelseReguleringsplanforslagType> Oversendelse { get; }
        public List<Attachment> Attachments { get; } = new();

        private RpfValidationInput(IEnumerable<GmlDocument> plankart2d, GmlDocument plankart3d, Kodelister kodelister)
        {
            Plankart2D.AddRange(plankart2d ?? new List<GmlDocument>());
            Plankart3D = plankart3d;
            Kodelister = kodelister;
        }

        public static IRpfValidationInput Create(IEnumerable<GmlDocument> plankart2d, GmlDocument plankart3d, Kodelister kodelister)
        {
            return new RpfValidationInput(plankart2d, plankart3d, kodelister);
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
                    Plankart2D.ForEach(plankart => plankart.Dispose());
                    Plankart3D?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}