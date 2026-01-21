using System.ComponentModel;
using System.Reflection;

namespace BuildingBlocks.Kernel.Extensions;

public static class EnumExtensions
{
    public static string? GetEnumDescription(this Enum? value, bool useNameAsFallback = false)
    {
        if (value == null)
            return null;

        FieldInfo? fi = value.GetType().GetField(value.ToString());

        if (fi != null)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
                return attributes[0].Description;
        }

        return useNameAsFallback ? value.ToString() : null;
    }
}