namespace Caisy.Web.Infrastructure.Models;

public class UserProfile : BaseEntity<UserProfile>, IUser
{
    public bool PrefersDarkMode { get; set; }
    public CodeBlockTheme CodeBlockTheme => PrefersDarkMode ? CodeBlockTheme.AtomOneDark : CodeBlockTheme.AtomOneLight;
}
