namespace Caisy.Web.Infrastructure;

public interface IIdentityProvider
{
    public IUser? User { get; set; }
}

public class IdentityProvider : IIdentityProvider
{
    public IUser? User { get; set; }
}

public interface IUser
{
    public string ApiKey { get; set; }
    public long Id { get; set; }
    public bool PrefersDarkMode { get; set; }
    public CodeBlockTheme CodeBlockTheme { get; }
}
