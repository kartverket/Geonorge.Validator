using DiBK.RuleValidator.Extensions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Validation
{
    public interface IValidationService
    {
        Task<ValidationReport> ValidateAsync(List<IFormFile> xmlFiles, IFormFile xsdFile);
    }
}
