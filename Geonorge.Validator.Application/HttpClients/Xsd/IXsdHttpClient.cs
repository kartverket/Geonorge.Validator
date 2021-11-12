using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public interface IXsdHttpClient
    {
        Task<(string XmlNamespace, string XsdVersion)> GetXmlNamespaceAndXsdVersion(List<IFormFile> xmlFiles, IFormFile xsdFile);
    }
}
