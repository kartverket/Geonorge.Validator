using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.XmlSchema.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.XmlSchema
{
    public interface IXmlSchemaHttpClient
    {
        Task<XmlSchemaData> GetXmlSchemaFromInputDataAsync(DisposableList<InputData> inputData);
        Task<MemoryStream> FetchXmlSchemaAsync(Uri uri);
    }
}
