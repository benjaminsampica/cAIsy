namespace Caisy.Web.Infrastructure.Models;

public class UserProfile : IEntity
{
    public string Id { get; set; } = nameof(UserProfile) + Guid.NewGuid();
    public required string ApiKey { get; set; }
    public bool PrefersDarkMode { get; set; }
}
