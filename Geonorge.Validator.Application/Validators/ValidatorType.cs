using System.ComponentModel;

namespace Geonorge.Validator.Application.Validators
{
    public enum ValidatorType
    {
        [Description("Plangrense 5.0")]
        Plangrense,
        [Description("Reguleringsplanforslag 5.0")]
        Reguleringsplanforslag,
        [Description("GML 3.2.1")]
        GML_321,
        Undefined
    }
}
