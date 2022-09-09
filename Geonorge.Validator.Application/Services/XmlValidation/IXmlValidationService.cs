using DiBK.RuleValidator.Extensions;
using Geonorge.Validator.Application.Models.Data;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Services.XmlValidation
{
    public interface IXmlValidationService
    {
        Task<ValidationReport> ValidateAsync(Submittal submittal);
    }
}
