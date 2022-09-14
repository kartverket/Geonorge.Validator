using DiBK.RuleValidator;

namespace Geonorge.Validator.GeoJson.Extensions
{
    public static class JsonRuleExtensions
    {
        public static void AddMessage(this ExecutableRule rule, string message, string fileName, string jsonPath, int lineNumber, int linePosition)
        {
            rule.AddMessage(new RuleMessage
            {
                Message = message,
                Properties = new Dictionary<string, object>
                {
                    { "FileName", fileName },
                    { "JsonPath", jsonPath },
                    { "LineNumber", lineNumber },
                    { "LinePosition", linePosition }
                }
            });
        }

        public static void AddMessage(this ExecutableRule rule, string message, string fileName, string jsonPath, int lineNumber, int linePosition, string zoomTo)
        {
            rule.AddMessage(new RuleMessage
            {
                Message = message,
                Properties = new Dictionary<string, object>
                {
                    { "FileName", fileName },
                    { "JsonPath", jsonPath },
                    { "LineNumber", lineNumber },
                    { "LinePosition", linePosition },
                    { "ZoomTo", zoomTo },
                }
            });
        }
    }
}

