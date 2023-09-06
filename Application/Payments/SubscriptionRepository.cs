using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class SubscriptionRepository
{
    private readonly AiPluginDbContext context;

    public SubscriptionRepository(AiPluginDbContext context)
    {
        this.context = context;
    }

    public async Task<Subscription?> GetLastSubscriptionByUserId(string userId)
    {
        return await context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Subscription>> GetSubscriptionsByUserId(string userId)
    {
        return await context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedOn)
            .ToListAsync();
    }

    public async Task<Subscription> GetSubscription(string id)
    {
        return await context.Subscriptions
            .FindAsync(id) ?? throw new KeyNotFoundException(nameof(id));
    }

    public async Task UpsertSubscription(Subscription subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        var existingSubscription = await context.Subscriptions.FindAsync(subscription.SubscriptionId);
        if (existingSubscription == null)
        {
            await AddSubscription(subscription);
            return;
        }

        existingSubscription.CustomerId = subscription.CustomerId;
        existingSubscription.Status = subscription.Status;
        existingSubscription.ExpiresOn = subscription.ExpiresOn;

        await UpdateSubscription(existingSubscription);
        
        return;
    }

    private async Task AddSubscription(Subscription subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        await context.Subscriptions.AddAsync(subscription);

        var plugins = await context.Plugins
            .Where(p => p.UserId == subscription.UserId)
            .OrderByDescending(p => p.CreationDateTime)
            .AsTracking()
            .ToListAsync();
        foreach (var plugin in plugins)
        {
            plugin.IsActive = plugins.IndexOf(plugin) < 3 || subscription.Status == SubscriptionStatus.Active;
        }

        await context.SaveChangesAsync();
    }
    
    private async Task UpdateSubscription(Subscription subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        context.Subscriptions.Update(subscription);

        bool isPremium = await IsUserPremium(subscription.UserId);

        var plugins = await context.Plugins
            .Where(p => p.UserId == subscription.UserId)
            .OrderByDescending(p => p.CreationDateTime)
            .AsTracking()
            .ToListAsync();

        foreach (var plugin in plugins)
        {
            // Use the isPremium variable to determine if the user is premium
            plugin.IsActive = plugins.IndexOf(plugin) < 3 || isPremium;
        }
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsUserPremium(string userId)
    {
        var result = await context.Subscriptions         //take the subscriptions
            .Where(s => s.UserId == userId          //of the user
                && s.ExpiresOn > DateTime.UtcNow)   //that are not expired
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefaultAsync();                           //take the last added
        return result?.Status == SubscriptionStatus.Active;  //and check if it is active
    }
}