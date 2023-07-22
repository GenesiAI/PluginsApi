using Stripe;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Stripe.Checkout;

[Route("api/[controller]")]
public class PaymentsController : Controller
{
    [HttpGet("intent")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var userEmail = "";
        // TODO: Get the current user's email;
        var userStripeCustomerId = "";
        // TODO: Get the current user's Stripe customer ID from database if there is one, otherwise false is fine
        
        var priceId = "price_1NWcI7KxWQlpnUKopF95YKY3";
        StripeConfiguration.ApiKey = "sk_test_51NUpR6KxWQlpnUKojYJKdYUbkU7bLtIzrNcuQYfljorsomr5g1VRq5qbQUYgE7WiCExKkVpLEWRk8qpOsWkgXozZ00tM5Nl6M0";

        var options = new SessionCreateOptions
        {
            // See https://stripe.com/docs/api/checkout/sessions/create
            SuccessUrl = "https://genesi.ai/success",
            CancelUrl = "https://genesi.ai/manage-subscriptions/cancelled",
            Mode = "subscription",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = priceId,
                    // For metered billing, do not pass quantity
                    Quantity = 1,
                },
            },

            // customer options
            CustomerEmail = userEmail,
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session",
            },
        };
        if (!string.IsNullOrEmpty(userStripeCustomerId))
        {
            options.Customer = userStripeCustomerId;
        }

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // return the session ID to the frontend
        return Ok(new { checkoutSessionId = session.Id });
    }

    // This endpoint is used by Stripe to send webhook events
    [HttpPost("webhook")]
    /// <summary>
    /// This endpoint is used by Stripe to send webhook events
    public async Task<IActionResult> HandleWebhook()
    {
        return Ok();
    }

    // Additional methods and endpoints as needed...
/*
    private async Task<User> GetUserFromDatabase(string userId)
    {
        // Implement this method to fetch a user from your database
    }

    private async Task SaveUserToDatabase(User user)
    {
        // Implement this method to save a user to your database
    }

    private async Task<Customer> CreateStripeCustomer(User user)
    {
        // Implement this method to create a Stripe customer
    }

    private async Task<Subscription> CreateStripeSubscription(User user, string planId)
    {
        // Implement this method to create a Stripe subscription
    }*/
}
