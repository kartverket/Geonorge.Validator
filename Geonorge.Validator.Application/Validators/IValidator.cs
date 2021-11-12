﻿using DiBK.RuleValidator;
using DiBK.RuleValidator.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Geonorge.Validator.Application.Validators
{
    public interface IValidator
    {
        Task<List<Rule>> Validate(string xmlNamespace, DisposableList<InputData> inputData);
    }
}
