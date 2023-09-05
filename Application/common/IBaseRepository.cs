using AiPlugin.Domain.Common;
using AiPlugin.Domain.Plugin;

namespace AiPlugin.Application.common;

public interface IBaseRepository<T> where T : EntityBase
{
    public Task<T> Add(T entity, string userId, CancellationToken cancellationToken = default);
    public Task<T> Get(Guid id, CancellationToken cancellationToken = default);
    public Task<T> Update(T entity, CancellationToken cancellationToken = default);
    public Task Delete(Guid id, CancellationToken cancellationToken = default);
}