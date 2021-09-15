using System.ComponentModel;

namespace Geonorge.Validator.Application.Services.Validators
{
    public enum ValidatorType
    {
        [Description("Planomriss v5.0")]
        Planomriss,
        [Description("Reguleringsplanforslag v5.0")]
        Reguleringsplanforslag,
        Undefined
    }
}
