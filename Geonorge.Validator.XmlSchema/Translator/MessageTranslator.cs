using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Geonorge.Validator.XmlSchema.Translator
{
    internal class MessageTranslator
    {
        public static string TranslateError(string message)
        {
            if (Translate(message, Translations.InvalidChild, out var translation))
                return $"{translation}{AddTranslations(message, Translations.ListOfPossibleElements)}.{AddTranslations(message, Translations.OtherElements)}.";
           
            if (Translate(message, Translations.IncompleteContent, out translation))
                return $"{translation}{AddTranslations(message, Translations.ListOfPossibleElements)}.{AddTranslations(message, Translations.OtherElements)}.";

            if (Translate(message, Translations.CannotContainText, out translation))
                return $"{translation}{AddTranslations(message, Translations.ListOfPossibleElements)}.{AddTranslations(message, Translations.OtherElements)}.";

            if (Translate(message, Translations.InvalidElement, out translation))
                return $"{translation}{AddTranslation(message, Translations.InvalidValue)}";

            if (Translate(message, Translations.InvalidAttribute, out translation))
                return $"{translation}{AddTranslation(message, Translations.InvalidValue)}{AddTranslation(message, Translations.InvalidCharacter)}";

            if (Translate(message, Translations.InvalidChildWithoutNamesapce, out translation))
                return translation;

            if (Translate(message, Translations.RequiredAttributeMissing, out translation))
                return translation;

            if (Translate(message, Translations.AttributeNotDeclared, out translation))
                return translation;

            if (Translate(message, Translations.DefaultAttributeCouldNotBeApplied, out translation))
                return translation;

            if (Translate(message, Translations.TextOnly, out translation))
                return translation;

            if (Translate(message, Translations.AttributeNotEqualFixedValue, out translation))
                return translation;

            if (Translate(message, Translations.TagMismatch, out translation))
                return translation;

            if (Translate(message, Translations.GlobalElementDeclared, out translation))
                return translation;

            if (Translate(message, Translations.ElementNotDeclared, out translation))
                return translation;

            return message;
        }

        public static string TranslateWarning(string message)
        {
            if (Translate(message, Translations.SchemaNotFound, out var translation))
                return translation;

            if (Translate(message, Translations.NoSchemaInfoElement, out translation))
                return translation;

            if (Translate(message, Translations.NoSchemaInfoAttribute, out translation))
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
