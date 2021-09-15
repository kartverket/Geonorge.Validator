using Geonorge.Validator.Application.Models.Report;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services
{
    public interface IValidationService
    {
        ValidationReport Validate(List<IFormFile> files, string xmlNamespace);
    }
}
