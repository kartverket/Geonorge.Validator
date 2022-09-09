using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.JsonValidation
{
    public interface IJsonValidationService
    {
        Task<ValidationReport> ValidateAsync(Submittal submittal);
    }
}
