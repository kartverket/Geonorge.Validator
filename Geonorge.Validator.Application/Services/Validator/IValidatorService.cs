using Geonorge.Validator.Application.Models.Validator;
using System.Collections.Generic;

namespace Geonorge.Validator.Application.Services.Validator
{
    public interface IValidatorService
    {
        List<ValidatorInfo> GetValidatorInfo();
    }
}
