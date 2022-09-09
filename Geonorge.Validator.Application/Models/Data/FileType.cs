﻿using System.ComponentModel;

namespace Geonorge.Validator.Application.Models.Data
{
    public enum FileType
    {
        [Description("application/xml")]
        XML,
        [Description("application/xml+gml")]
        GML32,
        [Description("application/xml")]
        XSD,
        [Description("application/json")]
        JSON,
        [Description("application/octet-stream")]
        Unknown
    }
}
