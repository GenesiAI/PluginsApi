using AiPlugin.Domain.Plugin;

namespace AiPlugin.Api.Dto;
public class PluginsResponse
{
    public int PluginsCount { get; set; }
    public int MaxPlugins { get; set; }
    public IEnumerable<Plugin> Plugins { get; set; } = new List<Plugin>();
}
