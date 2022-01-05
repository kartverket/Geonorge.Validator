using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.MultipartRequest
{
    public class MultipartRequestService : IMultipartRequestService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MultipartRequestService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<InputFiles> GetFilesFromMultipart()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var reader = new MultipartReader(request.GetMultipartBoundary(), request.Body);
            var inputFiles = new InputFiles();
            MultipartSection section;

            while ((section = await reader.ReadNextSectionAsync()) != null)
            {
                if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition) || !contentDisposition.IsFileDisposition())
                    continue;

                var name = contentDisposition.Name.Value;

                if (name != "xmlFiles" && name != "xsdFile")
                    continue;

                var fileType = await FileHelper.GetFileType(section);

                if (name == "xmlFiles" && (fileType == FileType.XML || fileType == FileType.GML32))
                    inputFiles.XmlFiles.Add(await CreateFormFile(contentDisposition, section));

                else if (name == "xsdFile" && inputFiles.XsdFile == null && fileType == FileType.XSD)
                    inputFiles.XsdFile = await CreateFormFile(contentDisposition, section);
            }

            return inputFiles;
        }

        private static async Task<IFormFile> CreateFormFile(ContentDispositionHeaderValue contentDisposition, MultipartSection section)
        {
            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);
            await section.Body.DisposeAsync();
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, contentDisposition.Name.ToString(), contentDisposition.FileName.ToString())
            {
                Headers = new HeaderDictionary(),
                ContentType = section.ContentType
            };
        }
    }
}
