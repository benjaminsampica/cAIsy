namespace Caisy.Web.Infrastructure.Models;

public abstract class BaseEntity<T> where T : class
{
    public string Id { get; set; } = nameof(T) + Guid.NewGuid();
}