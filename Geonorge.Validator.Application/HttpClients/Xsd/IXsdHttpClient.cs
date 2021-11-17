using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public interface IXsdHttpClient
    {
        Task<Stream> GetXsdFromXmlFiles(List<IFormFile> xmlFiles);
    }
}
