using Geonorge.Validator.Application.Models.Validator;
using Geonorge.Validator.Application.Services.Validators;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services
{
    public interface IValidatorService
    {
        List<ValidatorInfo> GetValidatorInfo();
        IValidator GetValidator(string xmlNamespace);
    }
}
