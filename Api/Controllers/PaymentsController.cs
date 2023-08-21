using Stripe;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using AuthBase.Controllers;

[Route("api/[controller]")]
public class PaymentsController : AuthController
{
    const string endpointSecret = "whsec_c5eec0c1be214329f59549fb146423d6acb33d75363f4eab0541acf1d52c190f";
    public PaymentsController(SubscriptionRepository subscriptionRepository)
        : base(subscriptionRepository)
    {
    }

    [HttpGet("intent")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var userId = GetUserId();
        // customer can be null if it's the first time that the user is subscribing
        
        var priceId = "price_1NWcI7KxWQlpnUKopF95YKY3";
        StripeConfiguration.ApiKey = "sk_test_51NUpR6KxWQlpnUKojYJKdYUbkU7bLtIzrNcuQYfljorsomr5g1VRq5qbQUYgE7WiCExKkVpLEWRk8qpOsWkgXozZ00tM5Nl6M0";

        // do not recycle existing pending checkout: https://stripe.com/docs/api/checkout/sessions#:~:text=We%20recommend%20creating%20a%20new%20Session%20each%20time%20your%20customer%20attempts%20to%20pay.

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
            }
        };
        var customer = await _subscriptionRepository.GetCustomerByUserId(userId);
        if (customer != null)
        {
            options.Customer = customer.CustomerId;
        }

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        // save row in checkouts
        var checkout = new Checkout
        {
            CheckoutSessionId = session.Id,
            UserId = userId,
            Status = "pending",
        };
        await _subscriptionRepository.AddCheckout(checkout);

        // if GET was requested using flag ?automatic=false, return the session id; otherwise redirect to the checkout page
        if (Request.Query.ContainsKey("automatic") && Request.Query["automatic"] == "false")
        {
            return Ok(new { checkoutSessionId = session.Id });
        }
        else
        {
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }

    // This endpoint is used by Stripe to send webhook events
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], endpointSecret);

            // Handle the event
            switch (stripeEvent.Type) {
                case Events.CheckoutSessionExpired:
                // user has abandoned the checkout
                // mark the checkout as expired in the database
                var sessionToDelete = stripeEvent.Data.Object as Session;
                var checkoutToDelete = await _subscriptionRepository.GetCheckout(sessionToDelete.Id);
                checkoutToDelete.Status = "expired";
                await _subscriptionRepository.UpdateCheckout(checkoutToDelete);
                break;

                case Events.CheckoutSessionCompleted:
                // user has created their session by paying
                var sessionCreated = stripeEvent.Data.Object as Session;
                // find the session row in the database
                var completedCheckout = await _subscriptionRepository.GetCheckout(sessionCreated.Id);
                // update the checkout row in the database
                completedCheckout.Status = "completed";
                await _subscriptionRepository.UpdateCheckout(completedCheckout);
                // we now know the UserId
                var userId = completedCheckout.UserId;
                // find the customer row in the database
                var customer = await _subscriptionRepository.GetCustomerByUserId(userId);
                // if the customer row doesn't exist, create it
                if (customer == null)
                {
                    var customerId = sessionCreated.Customer.Id;
                    var newCustomer = new Customer
                    {
                        UserId = userId,
                        CustomerId = customerId,
                    };
                    await _subscriptionRepository.AddCustomer(newCustomer);
                }
                // find the subscription row in the database
                var subscription = await _subscriptionRepository.GetSubscriptionByUserId(userId);
                // if the subscription row doesn't exist, create it
                if (subscription == null)
                {
                    var subscriptionId = sessionCreated.Subscription.Id;
                    var status = sessionCreated.Subscription.Status;
                    var expiresOn = sessionCreated.Subscription.CurrentPeriodEnd;
                    var eventTime = stripeEvent.Created;
                    var newSubscription = new Subscription
                    {
                        SubscriptionId = subscriptionId,
                        UserId = userId,
                        CustomerId = customer.CustomerId,
                        Status = status,
                        ExpiresOn = expiresOn,
                        EventTime = eventTime,
                    };
                    await _subscriptionRepository.AddSubscription(newSubscription);
                }
                // if the subscription row does exist, update it
                else
                {
                    var status = sessionCreated.Subscription.Status;
                    var expiresOn = sessionCreated.Subscription.CurrentPeriodEnd;
                    var eventTime = stripeEvent.Created;
                    subscription.Status = status;
                    subscription.ExpiresOn = expiresOn;
                    subscription.EventTime = eventTime;
                    await _subscriptionRepository.UpdateSubscription(subscription);
                }
                break;
                
                case Events.CustomerSubscriptionCreated: case Events.CustomerSubscriptionDeleted: case Events.CustomerSubscriptionUpdated:
                // log the whole object to the console
                Console.WriteLine(stripeEvent.Data.Object);
                break;
                default:
                Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                break;
            }
            return Ok();
        }
        catch (StripeException e)
        {
            return BadRequest();
        }
    }
}
