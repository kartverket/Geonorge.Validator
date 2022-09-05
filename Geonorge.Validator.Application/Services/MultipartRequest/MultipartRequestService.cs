using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public async Task<Submittal> GetFilesFromMultipart()
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var reader = new MultipartReader(request.GetMultipartBoundary(), request.Body);
            var sumbittal = new Submittal();
            var formAccumulator = new KeyValueAccumulator();
            MultipartSection section;

            try
            {
                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                        continue;

                    var name = contentDisposition.Name.Value;

                    if (contentDisposition.IsFileDisposition() && (name == "files" || name == "schema"))
                    {
                        var fileType = await FileHelper.GetFileType(section);

                        if (name == "files" && (fileType == FileType.XML || fileType == FileType.GML32))
                            sumbittal.Files.Add(await CreateFormFile(contentDisposition, section));

                        else if (name == "schema" && sumbittal.Schema == null && fileType == FileType.XSD)
                            sumbittal.Schema = await CreateFormFile(contentDisposition, section);
                    }
                    else if (contentDisposition.IsFormDisposition() && name == "skipRules")
                    {
                        formAccumulator = await AccumulateForm(formAccumulator, section, contentDisposition);
                    }
                }

                sumbittal.SkipRules = GetSkippedRules(formAccumulator);

                return sumbittal;
            }
            catch
            {
                return null;
            }
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

        private static async Task<KeyValueAccumulator> AccumulateForm(
            KeyValueAccumulator formAccumulator, MultipartSection section, ContentDispositionHeaderValue contentDisposition)
        {
            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

            using var streamReader = new StreamReader(section.Body, GetEncoding(section), true, 1024, true);
            {
                var value = await streamReader.ReadToEndAsync();

                if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                    value = string.Empty;

                formAccumulator.Append(key, value);

                if (formAccumulator.ValueCount > FormReader.DefaultValueCountLimit)
                    throw new InvalidDataException($"Form key count limit {FormReader.DefaultValueCountLimit} exceeded.");
            }

            return formAccumulator;
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            #pragma warning disable SYSLIB0001
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
                return Encoding.UTF8;
            #pragma warning restore SYSLIB0001

            return mediaType.Encoding;
        }

        private static List<string> GetSkippedRules(KeyValueAccumulator formAccumulator)
        {
            var accumulatedValues = formAccumulator.GetResults();
            accumulatedValues.TryGetValue("skipRules", out var value);
            var idListString = value.ToString();

            if (string.IsNullOrWhiteSpace(idListString))
                return new();

            return idListString
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .ToList();
        }
    }
}
