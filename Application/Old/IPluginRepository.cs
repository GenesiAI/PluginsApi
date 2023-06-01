using AiPlugin.Domain;

namespace AiPlugin.Application.Old
{
    public interface IPluginRepository
    {
        Task<Plugin> CreatePlugin(string userId, string content);
        Task<Plugin> GetPlugin(string userId, Guid pluginId);
        Task<Section> GetSection(string userId, Guid pluginId, Guid sectionIdc);
    }
}