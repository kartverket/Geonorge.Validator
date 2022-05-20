namespace Geonorge.XsdValidator.Translator
{
    internal static class Translations
    {
        public static Translation InvalidChild = new(
            @"^The element '(?<element>[^ ]*)' in namespace '(?<ns>[^ ]*)' has invalid child element '(?<childElement>[^ ]*)' in namespace '(?<childNs>[^ ]*)'.",
            "Elementet '{element}' i navneområdet '{ns}' har ugyldig barnelement '{childElement}' i navneområdet '{childNs}'."
        );

        public static Translation InvalidChildWithoutNamesapce = new(
            @"^The element '(?<element>[^ ]*)' in namespace '(?<ns>[^ ]*)' has invalid child element '(?<childElement>[^ ]*)'.",
            "Elementet '{element}' i navneområdet '{ns}' har ugyldig barnelement '{childElement}'."
        );

        public static Translation IncompleteContent = new(
            @"^The element '(?<element>[^ ]*)' in namespace '(?<ns>[^ ]*)' has incomplete content.",
            "Elementet '{element}' i navneområdet '{ns}' har ufullstendig innhold."
        );

        public static Translation CannotContainText = new(
            @"^The element '(?<element>[^ ]*)' in namespace '(?<ns>[^ ]*)' cannot contain text. List of possible elements expected: '(?<posElements>.*?)' in namespace '(?<posNs>.*?)'",
            "Elementet '{element}' i navneområdet '{ns}' kan ikke inneholde tekst."
        );

        public static Translation InvalidElement = new(
            @"^The '(?<element>[^ ]*)' element is invalid",
            "Elementet '{element}' er ugyldig."
        );

        public static Translation InvalidAttribute = new(
            @"^The '(?<attr>[^ ]*)' attribute is invalid",
            "Attributtet '{attr}' er ugyldig."
        );

        public static Translation InvalidValue = new(
            @" - The value '(?<value>.*?)' is invalid according to its datatype '(?<datatype>.*?)'",
            " Verdien '{value}' er ugyldig i henhold til dens datatype '{datatype}'."
        );

        public static Translation InvalidCharacter = new(
            @" - The '(?<character>.*?)' character, hexadecimal value (?<hexValue>.*), cannot be included in a name.",
            " Tegnet '{character}', heksadesimalverdi {hexValue}, kan ikke brukes som navn."
        );

        public static Translation RequiredAttributeMissing = new(
            @"^The required attribute '(?<attr>[^ ]*)' is missing.",
            "Det obligatoriske attributtet '{attr}' mangler."
        );

        public static Translation AttributeNotDeclared = new(
            @"^The '(?<attr>[^ ]*)' attribute is not declared.$",
            "Attributtet '{attr}' er ikke erklært."
        );

        public static Translation DefaultAttributeCouldNotBeApplied = new(
            @"^Default attribute '(?<attr>[^ ]*)' for element '(?<element>[^ ]*)' could not be applied as the attribute namespace is not mapped to a prefix in the instance document.$",
            "Standard-attributtet '{attr}' for elementet '{element}' kan ikke brukes, fordi attributtets navneområde ikke er tilordnet et prefiks i instansdokumentet."
        );

        public static Translation TextOnly = new(
            @"^The element '(?<element>[^ ]*)' cannot contain child element '(?<childElement>[^ ]*)' because the parent element's content model is text only.",
            "Elementet '{element}' kan ikke inneholde barnelement '{childElement}' fordi foreldreelementets innholdsmodell er kun tekst."
        );

        public static Translation AttributeNotEqualFixedValue = new(
            @"^The value of the '(?<attr>.*?)' attribute does not equal its fixed value\.$",
            "Verdien av attributtet '{attr}' er ikke lik dets faste verdi."
        );

        public static Translation TagMismatch = new(
            @"^The '(?<startTag>.*?)' start tag on line (?<startLine>\d+) position (?<startPos>\d+) does not match the end tag of '(?<endTag>.*?)'. Line (?<endLine>\d+), position (?<endPos>\d+).$",
            "Linje {startLine}, posisjon {startPos}: Start-taggen '{startTag}' matcher ikke slutt-taggen '{endTag}' på linje {endLine}, posisjon {endPos}."
        );

        public static Translation SchemaNotFound = new(
            @"^Cannot load the schema for the namespace '(?<ns>.*?)' - Could not find file '(?<file>.*?)'.$",
            "Kan ikke laste skjemaet for navneområdet '{ns}' - kunne ikke finne filen '{file}'."
        );

        public static Translation GlobalElementDeclared = new(
            @"^The global element '(?<element>[^ ]*)' has already been declared.$",
            "Det globale elementet '{element}' er allerede erklært."
        );

        public static Translation ElementNotDeclared = new(
            @"^The '(?<element>[^ ]*)' element is not declared.$",
            "Elementet '{element}' er ikke erklært."
        );

        public static Translation NoSchemaInfoElement = new(
            @"^Could not find schema information for the element '(?<element>[^ ]*)'.$",
            "Kunne ikke finne skjemainformasjon for elementet '{element}'."
        );

        public static Translation NoSchemaInfoAttribute = new(
            @"^Could not find schema information for the attribute '(?<attribute>[^ ]*)'.$",
            "Kunne ikke finne skjemainformasjon for attributtet '{attribute}'."
        );

        public static Translation ListOfPossibleElements = new(
            @" List of possible elements expected: '(?<posElements>.*?)' in namespace '(?<posNs>.*?)'",
            " Liste med mulige forventede elementer: '{posElements}' i navneområdet '{posNs}'"
        );

        public static Translation OtherElements = new(
            @" as well as '(?<otherElements>.*?)' in namespace '(?<otherNs>.*?)'",
            " i tillegg til '{otherElements}' i navneområdet '{otherNs}'"
        );
    }
}
