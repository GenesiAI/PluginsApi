using AiPlugin.Domain.Plugin;

namespace AiPlugin.Application.Plugins
{
    public interface IPluginWhitelistRepository
    {
        Task<PluginWhitelist> Get(Guid pluginId, string email, CancellationToken cancellationToken = default);
        Task<PluginWhitelist> Add(PluginWhitelist pluginWhitelist, string userId, CancellationToken cancellationToken = default);
        Task<PluginWhitelist> Update(PluginWhitelist pluginWhitelist, CancellationToken cancellationToken = default);
        Task Delete(Guid pluginId, string email, CancellationToken cancellationToken = default);
    }
}
