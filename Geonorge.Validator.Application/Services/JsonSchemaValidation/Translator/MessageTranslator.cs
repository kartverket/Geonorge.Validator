using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Geonorge.Validator.Application.Services.JsonSchemaValidation.Translator
{
    internal class MessageTranslator
    {
        public static string TranslateError(string message)
        {
            if (Translate(message, Translations.RequiredPropertiesMissing, out var translation))
                return translation;

            if (Translate(message, Translations.CouldNotValidateAgainstFormat, out translation))
                return translation;

            return message;
        }

        private static bool Translate(string message, Translation translation, out string output)
        {
            output = null;
            var match = translation.Regex.Match(message);

            if (!match.Success)
                return false;

            output = FormatMessage(translation.Template, match);
            return true;
        }

        private static string AddTranslation(string message, Translation translation)
        {
            return Translate(message, translation, out var translated) ? translated : "";
        }

        private static string AddTranslations(string message, Translation translation)
        {
            var translated = string.Empty;
            var matches = translation.Regex.Matches(message);

            foreach (Match match in matches)
                translated += FormatMessage(translation.Template, match);

            return translated;
        }

        private static string FormatMessage(string template, Match match)
        {
            var message = template;
            var values = GetGroupValues(match);

            foreach (var kvp in values)
                message = message.Replace($"{{{kvp.Key}}}", kvp.Value);

            return message;
        }

        private static IDictionary<string, string> GetGroupValues(Match match)
        {
            var values = new Dictionary<string, string>();

            foreach (Group group in match.Groups)
                values.Add(group.Name, group.Value);

            return values;
        }
    }
}
