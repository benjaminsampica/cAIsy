using Caisy.Web.Infrastructure.Extensions;

namespace Caisy.Web.Infrastructure.Models;

public abstract class BaseEntity<T> where T : class
{
    public long Id { get; set; } = Convert.ToInt64($"{LocalStorageExtensions.GetTableId<T>()}{Random.Shared.Next()}");
}
