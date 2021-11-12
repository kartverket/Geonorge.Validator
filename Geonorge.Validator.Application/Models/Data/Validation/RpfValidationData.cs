using DiBK.RuleValidator.Extensions;
using Reguleringsplanforslag.Rules;
using Reguleringsplanforslag.Rules.Models;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Oversendelse;
using SOSI.Produktspesifikasjon.Reguleringsplanforslag.Planbestemmelser;
using System;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Models
{
    public class RpfValidationData : IRpfValidationData
    {
        public List<GmlDocument> Plankart2D { get; } = new();
        public GmlDocument Plankart3D { get; }
        public Kodelister Kodelister { get; }
        public ValidationDataElement<ReguleringsplanbestemmelserType> Planbestemmelser { get; }
        public ValidationDataElement<OversendelseReguleringsplanforslagType> Oversendelse { get; }
        public List<Attachment> Attachments { get; } = new();

        private RpfValidationData(IEnumerable<GmlDocument> plankart2d, GmlDocument plankart3d, Kodelister kodelister)
        {
            Plankart2D.AddRange(plankart2d ?? new List<GmlDocument>());
            Plankart3D = plankart3d;
            Kodelister = kodelister;
        }

        public static IRpfValidationData Create(IEnumerable<GmlDocument> plankart2d, GmlDocument plankart3d, Kodelister kodelister)
        {
            return new RpfValidationData(plankart2d, plankart3d, kodelister);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            Plankart2D.ForEach(plankart => plankart.Dispose());
        }
    }
}
