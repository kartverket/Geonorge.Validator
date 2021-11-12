using Geonorge.Validator.Application.Models.Validator;
using Geonorge.Validator.Application.Utils;
using Geonorge.Validator.Application.Validators.Config;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Services.Validator
{
    public class ValidatorService : IValidatorService
    {
        private readonly ValidatorOptions _options;

        public ValidatorService(
            IOptions<ValidatorOptions> options)
        {
            _options = options.Value;
        }

        public List<ValidatorInfo> GetValidatorInfo()
        {
            return _options.Validators
                .OrderBy(validator => validator.ValidatorType)
                .Select(validator =>
                {
                    return new ValidatorInfo
                    {
                        Name = validator.ValidatorType.GetDescription(),
                        Namespace = validator.XmlNamespace,
                        FileTypes = validator.AllowedFileTypes
                    };
                })
                .ToList();
        }
    }
}
