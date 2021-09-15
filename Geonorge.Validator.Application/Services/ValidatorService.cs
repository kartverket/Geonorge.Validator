using Geonorge.Validator.Application.Models.Validator;
using Geonorge.Validator.Application.Services.Validators;
using Geonorge.Validator.Application.Services.Validators.Config;
using Geonorge.Validator.Application.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geonorge.Validator.Application.Services
{
    public class ValidatorService : IValidatorService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ValidatorOptions _options;

        public ValidatorService(
            IHttpContextAccessor httpContextAccessor,
            IOptions<ValidatorOptions> options)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public IValidator GetValidator(string xmlNamespace)
        {
            var validator = GetService(xmlNamespace);

            if (validator == null)
                throw new Exception($"Validator not found for namespace '{xmlNamespace}'!");

            return validator;
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

        private IValidator GetService(string xmlNamespace)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;
            var serviceType = _options.GetServiceType(xmlNamespace);

            return serviceType != null ?
                serviceProvider.GetService(serviceType) as IValidator :
                null;
        }
    }
}
