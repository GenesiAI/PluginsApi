using AiPlugin.Application.Plugins;
using System.Text.RegularExpressions;
using AiPlugin.Domain.Plugin;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace AiPlugin.Application.Plugins;

public class PluginRepository : IPluginRepository
{
    private readonly AiPluginDbContext dbContext;
    private readonly SubscriptionRepository subscriptionRepository;

    public PluginRepository(AiPluginDbContext dbContext, SubscriptionRepository subscriptionRepository)
    {
        this.dbContext = dbContext;
        this.subscriptionRepository = subscriptionRepository;
    }

    public async Task<Plugin> Add(Plugin entity, string userId, CancellationToken cancellationToken = default)
    {
        CheckPlugin(entity);
        if (await HasReachedPluginQuota(userId))
        {
            throw new Exception("Max plugins reached");
        }
        entity.IsActive = true;
        dbContext.Plugins.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public IQueryable<Plugin> Get(string userId, CancellationToken cancellationToken = default)
    {
        return Get(cancellationToken)
            .Where(x => x.UserId == userId)
            .AsQueryable();

    }

    public IQueryable<Plugin> Get(CancellationToken cancellationToken = default)
    {
        return dbContext
            .Plugins
            .Include(x => x.Sections)
            .Where(x => !x.isDeleted).AsQueryable();
    }

    public async Task<Plugin> Get(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Plugins.Include(x => x.Sections).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null || entity.isDeleted)
            throw new KeyNotFoundException($"Plugin with id {id} not found");
        entity.Sections = entity.Sections?.Where(x => !x.isDeleted);
        return entity;
    }

    public async Task<Plugin> GetByUserId(string userid, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Plugins.Include(x => x.Sections).SingleOrDefaultAsync(x => x.UserId == userid, cancellationToken);
        if (entity is null || entity.isDeleted)
            throw new KeyNotFoundException($"Plugin with userid {userid} not found");
        entity.Sections = entity.Sections?.Where(x => !x.isDeleted);
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

    public async Task<bool> HasReachedPluginQuota(string userId)
    {
        var countTask = Get().CountAsync();
        var isPremiumTask = subscriptionRepository.IsUserPremium(userId);
        await Task.WhenAll(countTask, isPremiumTask);
        return countTask.Result < (isPremiumTask.Result ? 10 : 3);
    }
    private void CheckPlugin(Plugin entity)
    {
        //run  [a-zA-Z][a-zA-Z0-9_]*
        if (!Regex.IsMatch(entity.NameForModel, "^[a-zA-Z][a-zA-Z0-9_]*$"))
            throw new ArgumentException("NameForModel must be in [a-zA-Z][a-zA-Z0-9_]* format");
    }
}
