using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Validator.Map.HttpClients.Proxy
{
    public interface IProxyHttpClient
    {
        Task<FileContentResult> GetAsync(string url);
    }
}
