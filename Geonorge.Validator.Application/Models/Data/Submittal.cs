using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Geonorge.Validator.Application.Models.Data
{
    public class Submittal
    {
        public Submittal(DisposableList<InputData> files, Stream schema, Uri schemaUri, List<string> skipRules, FileType fileType)
        {
            InputData = files;
            Schema = schema;
            SchemaUri = schemaUri;
            SkipRules = skipRules;
            FileType = fileType;
        }

        public Submittal()
        {
        }

        public DisposableList<InputData> InputData { get; private set; } = new();
        public Stream Schema { get; private set; }
        public Uri SchemaUri { get; private set; }
        public List<string> SkipRules { get; private set; } = new();
        public FileType FileType { get; private set; } = FileType.Unknown;
        public bool IsValid => InputData.Any() && FileType != FileType.Unknown;
    }
}
