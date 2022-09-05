using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.Validation
{
    public interface IValidationService
    {
        Task<ValidationReport> ValidateAsync(Submittal submittal);
    }
}
