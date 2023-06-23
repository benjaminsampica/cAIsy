namespace Caisy.Web.Infrastructure;

public interface IRepository<T> where T : BaseEntity<T>
{
    ValueTask<ICollection<T>> GetAllAsync(CancellationToken cancellationToken);
    ValueTask<T?> FindAsync(long id, CancellationToken cancellationToken);
    ValueTask AddAsync(T item, CancellationToken cancellationToken);
    ValueTask RemoveAsync(long id, CancellationToken cancellationToken);
}
