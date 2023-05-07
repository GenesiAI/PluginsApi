using AiPlugin.Domain;

namespace AiPlugin.Application.Plugins;

public interface IBaseRepository<T> where T : EntityBase
{
    public Task<T> Add(T entity, CancellationToken cancellationToken = default);
    public Task<T> Get(Guid id, CancellationToken cancellationToken = default);
    public Task<IEnumerable<T>> Get(CancellationToken cancellationToken = default);
    public Task<T> Update(T entity, CancellationToken cancellationToken = default);
    public Task Delete(Guid id, CancellationToken cancellationToken = default);
}