using System.Security.Claims;
using System.Text.RegularExpressions;
using AiPlugin.Domain.Plugin;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Application.Plugins;

public class PluginRepository : IPluginRepository
{
    private readonly AiPluginDbContext dbContext;
    private readonly SubscriptionRepository subscriptionRepository;
    private readonly AdminWhitelist adminWhitelist;

    public PluginRepository(AiPluginDbContext dbContext, SubscriptionRepository subscriptionRepository, AdminWhitelist adminWhitelist)
    {
        this.dbContext = dbContext;
        this.subscriptionRepository = subscriptionRepository;
        this.adminWhitelist = adminWhitelist;
    }

    public async Task<Plugin> Add(Plugin entity, string userId, ClaimsPrincipal? user = null, CancellationToken cancellationToken = default)
    {
        CheckPlugin(entity);
        if (await HasReachedPluginQuota(userId, user))
        {
            throw new Exception("Max plugins reached");
        }
        entity.IsActive = true;
        dbContext.Plugins.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<Plugin> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Plugins.Include(x => x.Sections).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || entity.isDeleted)
            throw new KeyNotFoundException($"Plugin with id {id} not found");
        entity.Sections = entity.Sections?.Where(x => !x.isDeleted).ToList();
        return entity;
    }

    public async Task<IEnumerable<Plugin>> GetByUserId(string userid, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Plugins
            .Include(x => x.Sections)
            .Where(x => x.UserId == userid && !x.isDeleted)
            .ToListAsync(cancellationToken);

        foreach (var plugin in entity)
        {
            plugin.Sections = plugin.Sections?.Where(x => !x.isDeleted);
        }
        return entity;
    }

    public async Task<Plugin> Update(Plugin entity, CancellationToken cancellationToken = default)
    {
        CheckPlugin(entity);
        dbContext.Plugins.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Plugins.FindAsync(id);
        if (entity is null)
            throw new KeyNotFoundException($"Plugin with id {id} not found");
        entity.isDeleted = true;
        dbContext.Plugins.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasReachedPluginQuota(string userId, ClaimsPrincipal? user = null)
    {
        var userEmail = user?.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail != null && adminWhitelist.Contains(userEmail))
        {
            return false;
        }
      
        return (await dbContext
                .Plugins
                .Include(x => x.Sections)
                .Where(x => !x.isDeleted)
                .CountAsync(x => x.UserId == userId)
                ) >= await maxPlugins(userId,user);
    }
    public async Task<int> maxPlugins(string userId, ClaimsPrincipal? user = null)
    {
        var userEmail = user?.FindFirst(ClaimTypes.Email)?.Value;
        if (userEmail != null && adminWhitelist.Contains(userEmail))
        {
            return 10000;
        }
        var isPremium = await subscriptionRepository.IsUserPremium(userId);
        return isPremium ? 3 : 1;
    }
    private void CheckPlugin(Plugin entity)
    {
        //run  [a-zA-Z][a-zA-Z0-9_]*
        if (!Regex.IsMatch(entity.NameForModel, "^[a-zA-Z][a-zA-Z0-9_]*$"))
            throw new ArgumentException("NameForModel must be in [a-zA-Z][a-zA-Z0-9_]* format");
    }
}
