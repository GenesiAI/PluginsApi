using AiPlugin.Domain;

namespace AiPlugin.Application.Old
{
    public interface IPluginRepository
    {
        Task<Plugin> CreatePlugin(Guid userId, string content);
        Task<Plugin> GetPlugin(Guid userId, Guid pluginId);
        Task<Section> GetSection(Guid userId, Guid pluginId, Guid sectionIdc);
    }
}