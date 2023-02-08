using Geonorge.Validator.Application.Models.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.MultipartRequest
{
    public interface IMultipartRequestService
    {
        Task<Submittal> GetFilesFromMultipartAsync();
        Task<IFormFile> GetGmlFileFromMultipartAsync();
    }
}
