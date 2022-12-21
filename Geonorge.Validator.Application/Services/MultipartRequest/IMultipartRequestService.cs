using Geonorge.Validator.Application.Models.Data;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.MultipartRequest
{
    public interface IMultipartRequestService
    {
        Task<Submittal> GetFilesFromMultipartAsync();
    }
}
