using System.ComponentModel.DataAnnotations;

namespace AiPlugin.Api.Dto;
public class PluginsGetResponse
{
    public int PluginsCount { get; set; }
    public int MaxPlugins { get; set; }
    public IEnumerable<AppPlugin> Plugins { get; set; }
}