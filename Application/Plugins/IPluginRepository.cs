using AiPlugin.Domain;

namespace AiPlugin.Application.Plugins;

public interface IPluginRepository : IBaseRepository<Plugin>
{
    public IQueryable<Plugin> Get(string userId, CancellationToken cancellationToken = default);

}