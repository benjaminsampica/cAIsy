namespace Caisy.Web.Features.Shared;

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
    public string Id { get; set; }
    public bool PrefersDarkMode { get; set; }
    public CodeBlockTheme CodeBlockTheme { get; }
}
