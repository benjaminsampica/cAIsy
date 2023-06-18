namespace Caisy.Web.Infrastructure.Models;

public class UserProfile : BaseEntity<UserProfile>
{
    public required string ApiKey { get; set; }
    public bool PrefersDarkMode { get; set; }
}
