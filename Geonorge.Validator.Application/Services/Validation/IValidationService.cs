using Geonorge.Validator.Application.Models.Report;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Validation
{
    public interface IValidationService
    {
        Task<ValidationReport> Validate(List<IFormFile> xmlFiles, IFormFile xsdFile);
    }
}
