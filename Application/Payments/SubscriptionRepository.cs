using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

// public interface ISubscriptionRepository{

// }

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

    public async Task AddSubscription(Subscription subscription)
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
    // public async Task UpdateSubscription(Subscription subscription)
    // {
    //     ArgumentNullException.ThrowIfNull(subscription);
    //     context.Subscriptions.Update(subscription);

    //     var plugins = await context.Plugins
    //         .Where(p => p.UserId == subscription.UserId)
    //         .OrderByDescending(p => p.CreationDateTime)
    //         .AsTracking()
    //         .ToListAsync();

    //     foreach (var plugin in plugins)
    //     {
    //         plugin.IsActive = plugins.IndexOf(plugin) < 3 || subscription.Status == SubscriptionStatus.Active;
    //     }
    //     await context.SaveChangesAsync();
    // }


    // public async Task CreateOrUpdateSubscription(Subscription SubscriptiontoSearch)
    // {
    //     var subscription = await context.Subscriptions.FindAsync(SubscriptiontoSearch.Id);
    //     if (subscription == null)
    //     {
    //         await AddSubscription(SubscriptiontoSearch);
    //         return;
    //     }

    //     subscription.CustomerId = SubscriptiontoSearch.CustomerId;
    //     subscription.Status = SubscriptiontoSearch.Status;
    //     subscription.ExpiresOn = SubscriptiontoSearch.ExpiresOn;

    //     await UpdateSubscription(subscription);
    //     return;
    // }

    public async Task<bool> IsUserPremium(string userId)
    {
        var result = await context.Subscriptions         //take the subscriptions
            .Where(s => s.UserId == userId          //of the user
                && s.ExpiresOn > DateTime.UtcNow)   //that are not expired
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefaultAsync();                           //take the last added
        return result?.Status == SubscriptionStatus.Active;  //and check if it is active
    }

    //#region Checkout
    //public async Task AddCheckout(Checkout checkout)
    //{
    //    ArgumentNullException.ThrowIfNull(checkout);
    //    await context.Checkouts.AddAsync(checkout);
    //    await context.SaveChangesAsync();
    //}

    //public async Task UpdateCheckout(Checkout checkout)
    //{
    //    ArgumentNullException.ThrowIfNull(checkout);
    //    context.Checkouts.Update(checkout);
    //    await context.SaveChangesAsync();
    //}

    //public async Task DeleteCheckout(string checkoutId)
    //{
    //    ArgumentNullException.ThrowIfNull(checkoutId);

    //    var checkout = await context.Checkouts.FindAsync(checkoutId);
    //    if (checkout == null)
    //    {
    //        throw new KeyNotFoundException(nameof(checkout));
    //    }

    //    context.Checkouts.Remove(checkout);
    //    await context.SaveChangesAsync();
    //}

    //public async Task<Checkout?> GetCheckout(string checkoutId)
    //{
    //    return await context.Checkouts
    //        .Where(c => c.CheckoutSessionId == checkoutId)
    //        .SingleOrDefaultAsync();
    //}

    //#endregion

}