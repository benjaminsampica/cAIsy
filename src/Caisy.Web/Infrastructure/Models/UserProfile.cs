using Caisy.Web.Features.Shared;

namespace Caisy.Web.Infrastructure.Models;

public class UserProfile : BaseEntity<UserProfile>, IUser
{
    public required string ApiKey { get; set; }
    public bool PrefersDarkMode { get; set; }
    public CodeBlockTheme CodeBlockTheme => PrefersDarkMode ? CodeBlockTheme.AtomOneDark : CodeBlockTheme.AtomOneLight;
}
