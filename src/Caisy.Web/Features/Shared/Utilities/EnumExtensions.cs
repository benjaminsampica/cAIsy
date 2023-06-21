using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Caisy.Web.Features.Shared.Utilities;

public static class EnumExtensionss
{
    public static string GetDisplayName(this Enum @enum)
    {
        var enumName = @enum.ToString();

        return @enum.GetType()
           .GetMember(enumName)
           .First()
           .GetCustomAttribute<DisplayAttribute>()?
           .Name ?? enumName;
    }

}
