using System;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.HttpClients.XmlSchemaCacher
{
    public interface IXmlSchemaCacherHttpClient
    {
        Task CacheSchemasAsync(Uri uri);
        Task<int> UpdateCacheAsync();
    }
}
