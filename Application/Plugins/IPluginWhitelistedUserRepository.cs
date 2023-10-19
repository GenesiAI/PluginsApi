using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AiPlugin.Domain.Plugin;

namespace AiPlugin.Application.Plugins
{
    public interface IPluginWhitelistedUserRepository
    {
        Task<PluginWhitelistedUser> Get(string email, CancellationToken cancellationToken = default);
        Task<PluginWhitelistedUser> Add(PluginWhitelistedUser pluginWhitelistedUser, CancellationToken cancellationToken = default);
        Task<PluginWhitelistedUser> Update(PluginWhitelistedUser pluginWhitelistedUser, CancellationToken cancellationToken = default);
        Task Delete(string email, CancellationToken cancellationToken = default);
    }
}
