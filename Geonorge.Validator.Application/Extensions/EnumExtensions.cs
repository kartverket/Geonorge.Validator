using System;
using System.ComponentModel;

namespace Geonorge.Validator.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                return attribute.Description;
            
            throw new ArgumentException("Item not found.", nameof(enumValue));
        }
    }
}
