using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

[Route("api/[controller]")]
public class PaymentsController : AiPlugin.Api.Controllers.ControllerBase
{
    private readonly ILogger<PaymentsController> logger;
    private readonly SubscriptionRepository subscriptionRepository;
    private readonly StripeSettings stripeSettings;
    private readonly string frontendUrl;

    public PaymentsController(ILogger<PaymentsController> logger, SubscriptionRepository subscriptionRepository,
        StripeSettings stripeSettings, string frontendUrl) : base()
    {
        this.logger = logger;
        this.subscriptionRepository = subscriptionRepository;
        this.stripeSettings = stripeSettings;
        this.frontendUrl = frontendUrl;
    }

    [Authorize]
    [HttpGet("intent")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var userId = GetUserId();
        logger.LogInformation($"Creating checkout session for user {userId}");
        StripeConfiguration.ApiKey = stripeSettings.ApiKey;

<<<<<<< HEAD
        var isPremium = await subscriptionRepository.IsUserPremium(userId);
        if (isPremium)
        {
            return BadRequest("User is already premium");
        }

        // customer can be null if it's the first time that the user is subscribing
        // recycle existing pending checkout when possible
        var pendingCheckout = await subscriptionRepository.GetCheckout(userId);
        var checkoutSessionId = pendingCheckout?.CheckoutSessionId;
        if (checkoutSessionId != null)
        {
            return Ok(new { checkoutSessionId });
        }
=======
        // do not recycle existing pending checkout: https://stripe.com/docs/api/checkout/sessions#:~:text=We%20recommend%20creating%20a%20new%20Session%20each%20time%20your%20customer%20attempts%20to%20pay.
>>>>>>> 93c54ccc1088e955f6e683c8c8093e508ce7f392

        var options = await BuildSessionOptions(userId);
        var session = await new SessionService().CreateAsync(options);

        // save row in checkouts
        var checkout = new Checkout
        {
            CheckoutSessionId = session.Id,
            UserId = userId,
            Status = CheckoutStatuses.Pending,
        };
        await subscriptionRepository.AddCheckout(checkout);

        // if GET was requested using flag ?automatic=false, return the session id; otherwise redirect to the checkout page
        if (Request.Query.ContainsKey("automatic") && Request.Query["automatic"] == "false")
        {
            return Ok(new { checkoutSessionId = session.Id });
        }
        return Ok(session.Url);
    }

    private async Task<SessionCreateOptions> BuildSessionOptions(string userId)
    {
        var lastSubscription = await subscriptionRepository.GetLastSubscriptionByUserId(userId);
        var options = new SessionCreateOptions
        {
            SuccessUrl = $"{frontendUrl}success",
            CancelUrl = $"{frontendUrl}manage-subscriptions/cancelled",
            Mode = "subscription",
            Customer = lastSubscription?.CustomerId,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = stripeSettings.PremiumPriceId,
                    // For metered billing, do not pass quantity
                    Quantity = 1,
                },
            }
        };
        return options;
    }

    /// <summary>
    /// This endpoint is called by Stripe to send events
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] string content)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(content, Request.Headers["Stripe-Signature"], stripeSettings.WebHookSecret);
            logger.LogInformation($"Webhook called with type {stripeEvent.Type}");

            switch (stripeEvent.Type)
            {
                case Events.CheckoutSessionExpired:
                    // if the checkout session has expired, mark the checkout as expired

                    var checkout = await GetCheckoutFromEvent(stripeEvent);
                    if (checkout == null)
                    {
                        return BadRequest();
                    }
                    checkout.Status = CheckoutStatuses.Failed;
                    await subscriptionRepository.UpdateCheckout(checkout);
                    return Ok();    // return a 200 response so that Stripe doesn't retry the webhook

                case Events.CheckoutSessionCompleted:

                    var completedCheckout = await GetCheckoutFromEvent(stripeEvent);
                    if (completedCheckout == null)
                    {
                        return BadRequest();
                    }
                    completedCheckout.Status = CheckoutStatuses.Success;
                    await subscriptionRepository.UpdateCheckout(completedCheckout);

                    var session = stripeEvent.Data.Object as Session;

                    var subscription = await subscriptionRepository.GetSubscription(session!.Subscription.Id);
                    if (subscription == null)
                    {
                        var newSubscription = new Subscription
                        {
                            Id = session!.Subscription.Id,
                            UserId = completedCheckout.UserId,
                            CustomerId = session!.Subscription.CustomerId,
                            Status = session.Subscription.Status.ToSubscriptionStatus(),
                            ExpiresOn = session.Subscription.CurrentPeriodEnd,
                            CreatedOn = stripeEvent.Created, //todo unix time, need to convert to uct?
                        };
                        await subscriptionRepository.AddSubscription(newSubscription);
                        return Ok();
                    }

                    subscription.CustomerId = session!.Subscription.CustomerId;
                    subscription.Status = session.Subscription.Status.ToSubscriptionStatus();
                    subscription.ExpiresOn = session.Subscription.CurrentPeriodEnd;

                    await subscriptionRepository.UpdateSubscription(subscription);
                    return Ok();

                //case Events.CustomerSubscriptionCreated:
                case Events.CustomerSubscriptionDeleted:
                case Events.CustomerSubscriptionPaused:
                case Events.CustomerSubscriptionPendingUpdateApplied:
                case Events.CustomerSubscriptionPendingUpdateExpired:
                case Events.CustomerSubscriptionTrialWillEnd:
                case Events.CustomerSubscriptionResumed:
                case Events.CustomerSubscriptionUpdated:
                    if (stripeEvent.Data.Object is not Session subscriptionEvent)
                    {
                        return BadRequest();
                    }
                    var subscriptionToUpdate = await subscriptionRepository.GetSubscription(subscriptionEvent.Id);
                    if (subscriptionToUpdate == null)
                    {
                        return BadRequest();
                    }
                    subscriptionToUpdate.Status = subscriptionEvent.Subscription.Status.ToSubscriptionStatus();
                    await subscriptionRepository.UpdateSubscription(subscriptionToUpdate);
                    break;
                default:
                    logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                    break;
            }
            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
    }
    private async Task<Checkout?> GetCheckoutFromEvent(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session?.Id == null)
        {
            logger.LogError("CheckoutSessionExpired: sessionToDelete.Id is null");
            return null;
        }
        var checkout = await subscriptionRepository.GetCheckout(session.Id);
        if (checkout == null)
        {
            logger.LogError($"CheckoutSessionExpired: checkoutToDelete is null for session {session.Id}");
            return null;
        }
        return checkout;
    }
}

