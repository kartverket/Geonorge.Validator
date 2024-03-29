﻿using System.Text.RegularExpressions;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation.Translator
{
    internal class Translation
    {
        public Regex Regex { get; private set; }
        public string Template { get; private set; }

        public Translation(string pattern, string template)
        {
            Regex = new Regex(pattern, RegexOptions.Compiled);
            Template = template;
        }
    }
}
