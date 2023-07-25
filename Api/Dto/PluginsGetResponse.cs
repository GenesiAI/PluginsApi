using System.ComponentModel.DataAnnotations;
using AiPlugin.Domain;

namespace AiPlugin.Api.Dto;
public class PluginsGetResponse
{
    public int PluginsCount { get; set; }
    public int MaxPlugins { get; set; }
    public IEnumerable<Plugin> Plugins { get; set; } = null!;
}