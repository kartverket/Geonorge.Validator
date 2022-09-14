namespace Geonorge.Validator.Application.Services.JsonSchemaValidation.Translator
{
    internal static class Translations
    {
        public static Translation RequiredPropertiesMissing = new(
            @"^Required properties are missing from object: (?<properties>.*?)\.$",
            "Påkrevde egenskaper mangler i objektet: {properties}."
        );

        public static Translation CouldNotValidateAgainstFormat = new(
            @"^(?<data_type>.*?) '(?<value>.*?)' does not validate against format '(?<format>.*?)'.$",
            "{data_type} '{value}' validerer ikke mot formatet '{format}'."
        );
    }
}
