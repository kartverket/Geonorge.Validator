using Geonorge.Validator.Application.Exceptions;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Geonorge.Validator.Application.Utils
{
    public class XmlHelper
    {
        public static async Task<XDocument> LoadXDocumentAsync(Stream xmlStream, LoadOptions loadOptions = LoadOptions.None)
        {
            try
            {
                var document = await XDocument.LoadAsync(xmlStream, loadOptions, default);
                xmlStream.Position = 0;

                return document;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception, "Ugyldig XML-dokument");
                throw new InvalidXsdException($"Ugyldig XML-dokument");
            }
        }

        public static string ReadLines(Stream stream, int numberOfLines)
        {
            if (numberOfLines < 1)
                throw new ArgumentException("numberOfLines må være større enn 0");

            var counter = 0;
            var stringBuilder = new StringBuilder(numberOfLines * 250);
            using var streamReader = new StreamReader(stream);

            while (counter++ < numberOfLines && !streamReader.EndOfStream)
                stringBuilder.Append(streamReader.ReadLine());

            return stringBuilder.ToString();
        }
    }
}
