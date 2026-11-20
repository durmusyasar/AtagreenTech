using System.Linq.Expressions;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Tests.Fakes;

/// <summary>Unit test için bellek içi <see cref="IRepository{T}"/>.</summary>
internal sealed class InMemoryRepository<T> : IRepository<T>
    where T : BaseEntity
{
    private readonly List<T> _items = [];

    public IReadOnlyList<T> Items => _items;

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.FromResult(entity);
    }

    public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _items.AddRange(entities);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
        // Bellek içi: aynı referans zaten listede.
    }

    public void Delete(T entity) => _items.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities)
    {
        foreach (var entity in entities.ToList())
            _items.Remove(entity);
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.FirstOrDefault(e => e.Id == id));

    public Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.AsQueryable().FirstOrDefault(predicate));

    public Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(_items.ToList());

    public Task<IReadOnlyList<T>> GetAllByFilterAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default
    ) => Task.FromResult<IReadOnlyList<T>>(_items.AsQueryable().Where(predicate).ToList());

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(_items.AsQueryable().Any(predicate));

    public Task<PagedResult<TResult>> ToPagedResultAsync<TResult>(
        Func<IQueryable<T>, IQueryable<TResult>> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    ) => throw new NotSupportedException();
}

/// <summary>Unit test için no-op <see cref="IUnitOfWork"/>.</summary>
internal sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public bool HasActiveTransaction => false;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
