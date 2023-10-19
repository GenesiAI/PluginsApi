using AiPlugin.Domain.Plugin;

namespace AiPlugin.Api.Dto
{
    public class PluginWhitelistCreateRequest
    {
        public Plugin Plugin { get; set; }
        public PluginWhitelistedUser WhitelistedUser { get; set; }
    }
}
