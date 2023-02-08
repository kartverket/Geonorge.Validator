using Geonorge.Validator.Map.Models.Map;
using Microsoft.AspNetCore.Http;

namespace Geonorge.Validator.Map.Services
{
    public interface IMapDocumentService
    {
        Task<MapDocument> CreateMapDocumentAsync(IFormFile file);
    }
}
