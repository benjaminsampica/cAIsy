﻿using Blazored.LocalStorage;
using Caisy.Web.Infrastructure.Extensions;

namespace Caisy.Web.Infrastructure;

public class Repository<T> : IRepository<T> where T : BaseEntity<T>
{
    private readonly ILocalStorageService _localStorageService;

    public Repository(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async ValueTask<T?> FindAsync(long id, CancellationToken cancellationToken)
    {
        return await _localStorageService.GetItemAsync<T>(id.ToString(), cancellationToken);
    }

    public async ValueTask<ICollection<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        var allKeys = await _localStorageService.KeysAsync(cancellationToken);

        var relevantKeys = allKeys.Where(k => k.Contains(LocalStorageExtensions.GetTableId<T>().ToString())).ToList();

        var allTopicsTasks = new List<Task<T>>();
        foreach (var key in relevantKeys)
        {
            var entityTask = _localStorageService.GetItemAsync<T>(key, cancellationToken).AsTask();
            allTopicsTasks.Add(entityTask);
        }

        var allTopics = await Task.WhenAll(allTopicsTasks);

        return allTopics;
    }

    public async ValueTask AddAsync(T entity, CancellationToken cancellationToken)
    {
        await _localStorageService.SetItemAsync(entity.Id.ToString(), entity, cancellationToken);
    }

    public async ValueTask RemoveAsync(long id, CancellationToken cancellationToken)
    {
        await _localStorageService.RemoveItemAsync(id.ToString(), cancellationToken);
    }
}
