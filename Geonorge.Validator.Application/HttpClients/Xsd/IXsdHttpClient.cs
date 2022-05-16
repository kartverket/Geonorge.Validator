using Geonorge.XsdValidator.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.Xsd
{
    public interface IXsdHttpClient
    {
        Task<XsdData> GetXsdFromXmlFilesAsync(List<IFormFile> xmlFiles);
        Task<int> UpdateCacheAsync();
    }
}
