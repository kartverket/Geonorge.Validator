using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Exceptions;
using Geonorge.Validator.Application.Models.Data;
using Geonorge.Validator.Common.Helpers;
using Geonorge.Validator.Common.Models;
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
            var formAccumulator = new KeyValueAccumulator();
            var files = new List<InputData>();
            Stream schema = null;
            var fileTypes = new HashSet<FileType>();
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

                        if (name == "files" && (fileType == FileType.XML || fileType == FileType.GML32 || fileType == FileType.JSON))
                        {
                            files.Add(await CreateInputDataAsync(contentDisposition, section));
                            fileTypes.Add(fileType);
                        }
                        else if (name == "schema" && schema == null && (fileType == FileType.XSD || fileType == FileType.JSON))
                            schema = await CreateStreamAsync(contentDisposition, section);
                    }
                    else if (contentDisposition.IsFormDisposition() && (name == "skipRules" || name == "schemaUri"))
                    {
                        formAccumulator = await AccumulateForm(formAccumulator, section, contentDisposition);
                    }
                }

                if (fileTypes.Count > 1)
                    throw new MultipartRequestException("Datasettet inneholder ulike filtyper.");

                return new Submittal(
                    files.ToDisposableList(), 
                    schema, 
                    GetSchemaUri(formAccumulator),
                    GetSkippedRules(formAccumulator),
                    fileTypes.Single()
                );
            }
            catch
            {
                return new();
            }
        }

        private static async Task<InputData> CreateInputDataAsync(ContentDispositionHeaderValue contentDisposition, MultipartSection section)
        {
            var memoryStream = await CreateStreamAsync(contentDisposition, section);

            return new InputData(memoryStream, contentDisposition.FileName.ToString(), contentDisposition.Name.ToString());
        }

        private static async Task<MemoryStream> CreateStreamAsync(ContentDispositionHeaderValue contentDisposition, MultipartSection section)
        {
            var memoryStream = new MemoryStream();
            await section.Body.CopyToAsync(memoryStream);
            await section.Body.DisposeAsync();
            memoryStream.Position = 0;

            return memoryStream;
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

        private static Uri GetSchemaUri(KeyValueAccumulator formAccumulator)
        {
            var accumulatedValues = formAccumulator.GetResults();
            accumulatedValues.TryGetValue("schemaUri", out var value);
            var uriString = value.ToString();

            if (string.IsNullOrWhiteSpace(uriString))
                return null;

            return Uri.TryCreate(uriString, UriKind.Absolute, out var schemaUri) ?
                schemaUri :
                null;
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
