﻿using DiBK.RuleValidator.Rules.Gml;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using Geonorge.Validator.Application.Utils;
using System.Linq;

namespace Geonorge.Validator.Application.Models
{
    public class GmlValidationData : IGmlValidationData
    {
        public List<GmlDocument> Surfaces { get; } = new();
        public List<GmlDocument> Solids { get; } = new();

        public static GmlValidationData Create(DisposableList<InputData> inputData)
        {
            var gmlDocuments = ValidationHelpers.GetValidationData(inputData, data => data
                .Select(data => GmlDocument.Create(data))
                .ToList());

            var validationData = new GmlValidationData();

            foreach (var document in gmlDocuments)
            {
                var dimensions = GmlHelper.GetDimensions(document.Document.Root);

                if (dimensions == 2)
                    validationData.Surfaces.Add(document);
                else if (dimensions == 3)
                    validationData.Solids.Add(document);
            }

            return validationData;
        }
    }
}
