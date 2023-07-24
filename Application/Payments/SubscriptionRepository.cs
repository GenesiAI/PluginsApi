using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class SubscriptionRepository
{
    private readonly AiPluginDbContext _context;

    public SubscriptionRepository(AiPluginDbContext context)
    {
        _context = context;
    }
    public async Task<Subscription> GetSubscriptionByUserId(string userId)
    {
        return await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.ExpiresOn)  // get the latest subscription
            .FirstOrDefaultAsync();
    }
    public async Task AddSubscription(Subscription subscription)
    {
        if (subscription == null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateSubscription(Subscription subscription)
    {
        if (subscription == null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
    }
    public async Task AddCheckout(Checkout checkout)
    {
        if (checkout == null)
        {
            throw new ArgumentNullException(nameof(checkout));
        }

        await _context.Checkouts.AddAsync(checkout);
        await _context.SaveChangesAsync();
    }
    // updatecheckout
    public async Task UpdateCheckout(Checkout checkout)
    {
        if (checkout == null)
        {
            throw new ArgumentNullException(nameof(checkout));
        }

        _context.Checkouts.Update(checkout);
        await _context.SaveChangesAsync();
    }
    // deletecheckout by checkoutid
    public async Task DeleteCheckout(string checkoutId)
    {
        var checkout = await _context.Checkouts.FindAsync(checkoutId);
        if (checkout == null)
        {
            throw new ArgumentNullException(nameof(checkout));
        }

        _context.Checkouts.Remove(checkout);
        await _context.SaveChangesAsync();
    }
    // AddCustomer
    public async Task<Customer> AddCustomer(Customer customer)
    {
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
    // updatecustomer
    public async Task<Customer> UpdateCustomer(Customer customer)
    {
        if (customer == null)
        {
            throw new ArgumentNullException(nameof(customer));
        }

        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
    // getcustomerbyuserid
    public async Task<Customer?> GetCustomerByUserId(string userId)
    {
        return await _context.Customers
            .Where(c => c.UserId == userId)
            .FirstOrDefaultAsync();
    }
    // get customer by customer id
    public async Task<Customer?> GetCustomerByCustomerId(string customerId)
    {
        return await _context.Customers
            .Where(c => c.CustomerId == customerId)
            .FirstOrDefaultAsync();
    }
    // getcheckout
    public async Task<Checkout?> GetCheckout(string checkoutId)
    {
        return await _context.Checkouts
            .Where(c => c.CheckoutSessionId == checkoutId)
            .FirstOrDefaultAsync();
    }
    // GetPendingCheckout
    public async Task<Checkout?> GetPendingCheckout(string userId)
    {
        return await _context.Checkouts
            .Where(c => c.UserId == userId && c.Status == "pending")
            .FirstOrDefaultAsync();
    }
}