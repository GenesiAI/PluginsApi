using System.Security.Claims;
using AiPlugin.Application.common;
using AiPlugin.Domain.Plugin;

namespace AiPlugin.Application.Plugins;

public interface IPluginRepository : IBaseRepository<Plugin>
{
    public Task<IEnumerable<Plugin>> GetByUserId(string userid, CancellationToken cancellationToken = default);
    public Task<bool> HasReachedPluginQuota(string userId, ClaimsPrincipal? user = null);
}