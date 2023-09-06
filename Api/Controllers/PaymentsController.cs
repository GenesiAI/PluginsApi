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
        logger.LogTrace($"Creating checkout session for user {userId}");
        StripeConfiguration.ApiKey = stripeSettings.ApiKey;

        var isPremium = await subscriptionRepository.IsUserPremium(userId);
        if (isPremium)
        {
            return BadRequest("User is already premium");
        }

        var options = BuildSessionOptions(userId);
        var session = await new SessionService().CreateAsync(options);

        // if GET was requested using flag ?automatic=false, return the session id; otherwise redirect to the checkout page
        if (Request.Query.ContainsKey("automatic") && Request.Query["automatic"] == "false")
        {
            return Ok(new { checkoutSessionId = session.Id });
        }
        return Ok(session.Url);
    }


    /// <summary>
    /// This endpoint is called by Frontend to unsubscribe a user, it simply tells stripe to mark the subscription as canceled
    /// So that the user can still use the premium features until the end of the billing period, but as soon as the billing period ends
    /// the webhook will be called and the subscription will be marked as canceled
    /// </summary>
    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe()
    {
        var userId = GetUserId();
        logger.LogTrace($"Unsubscribing user {userId}");
        StripeConfiguration.ApiKey = stripeSettings.ApiKey;

        var isPremium = await subscriptionRepository.IsUserPremium(userId);
        if (!isPremium)
        {
            return BadRequest("User is not premium");
        }

        var lastSubscription = await subscriptionRepository.GetLastSubscriptionByUserId(userId);
        if (lastSubscription == null)
        {
            return BadRequest("User has no subscription");
        }

        var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };
        var service = new SubscriptionService();
        service.Update(lastSubscription.SubscriptionId, options);

        return Ok();
    }

    /// <summary>
    /// This endpoint is called by Stripe to send events
    /// </summary>
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook()
    {
        try
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], stripeSettings.WebHookSecret);
            logger.LogInformation($"Webhook called with type {stripeEvent.Type}");

            switch (stripeEvent.Type)
            {
                case Events.CustomerSubscriptionDeleted:
                case Events.CustomerSubscriptionPaused:
                case Events.CustomerSubscriptionPendingUpdateApplied:
                case Events.CustomerSubscriptionPendingUpdateExpired:
                case Events.CustomerSubscriptionTrialWillEnd:
                case Events.CustomerSubscriptionResumed:
                case Events.CustomerSubscriptionUpdated:

                    if (stripeEvent.Data.Object is not Stripe.Subscription updatedSubscription)
                    {
                        return BadRequest("failed to cast event");
                    }
                    if (!updatedSubscription.Metadata.TryGetValue("UserId", out var updaterId))
                    {
                        return BadRequest("failed to get userid from metadata");
                    }

                    StripeConfiguration.ApiKey = stripeSettings.ApiKey;
                    var service = new SubscriptionService();
                    var subscription = await service.GetAsync(updatedSubscription.Id);
                    await subscriptionRepository.UpsertSubscription(
                        new Subscription()
                        {
                            SubscriptionId = subscription!.Id,
                            UserId = updaterId!,
                            CustomerId = subscription.CustomerId,
                            Status = subscription.Status.ToSubscriptionStatus(),
                            ExpiresOn = subscription.CurrentPeriodEnd,
                            CreatedOn = stripeEvent.Created
                        }
                    );
                    return Ok();

                default:
                    logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                    return Ok();
            }
        }
        catch (StripeException stripeException)
        {
            logger.LogCritical(stripeException, "Exception while trying to handle stripe event: " + stripeException.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception while trying to handle stripe event: " + ex.Message);
            throw;
        }
    }
    
    private SessionCreateOptions BuildSessionOptions(string userId)
    {
        // var lastSubscription = await subscriptionRepository.GetLastSubscriptionByUserId(userId);
        var options = new SessionCreateOptions
        {
            SuccessUrl = $"{frontendUrl}success",
            CancelUrl = $"{frontendUrl}manage-subscriptions/cancelled",
            Mode = "subscription",
            // Customer = lastSubscription?.CustomerId,
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Price = stripeSettings.PremiumPriceId,
                    // For metered billing, do not pass quantity
                    Quantity = 1,
                },
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId },
                },
            }
        };
        return options;
    }
}

