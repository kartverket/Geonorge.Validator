using Geonorge.Validator.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Geonorge.Validator.Common.Helpers
{
    public class JsonHelper
    {
        public static async Task<JToken> LoadJsonDocumentAsync(Stream stream)
        {
            try
            {
                using var jsonReader = new JsonTextReader(new StreamReader(stream, leaveOpen: true));
                stream.Position = 0;

                return await JToken.LoadAsync(jsonReader);
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Ugyldig JSON-dokument");
                throw new InvalidJsonException($"Ugyldig JSON-dokument");
            }
        }
    }
}
