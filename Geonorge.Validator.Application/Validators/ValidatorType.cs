using System.ComponentModel;

namespace Geonorge.Validator.Application.Validators
{
    public enum ValidatorType
    {
        [Description("Plangrense v5.0")]
        Plangrense,
        [Description("Reguleringsplanforslag v5.0")]
        Reguleringsplanforslag,
        Undefined
    }
}
