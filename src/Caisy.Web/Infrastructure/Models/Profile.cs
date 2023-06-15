namespace Caisy.Web.Infrastructure.Models;

public class Profile : IEntity
{
    public string Id { get; set; } = nameof(Profile) + Guid.NewGuid();
}
