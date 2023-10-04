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
        StripeConfiguration.ApiKey = stripeSettings.ApiKey;
    }

    [Authorize]
    [HttpGet("manage")]
    public async Task<IActionResult> Manage()
    {
        var userId = GetUserId();
        var lastSubscription = await subscriptionRepository.GetLastSubscriptionByUserId(userId);
        if (lastSubscription == null)
        {
            return BadRequest("User has no subscription");
        }

        string referrer = "";
        if (Request.Headers.ContainsKey("Referer"))
        {
            referrer = Request.Headers["Referer"].ToString();
        }
        if (!referrer.StartsWith(frontendUrl)) {
            referrer = frontendUrl;
        }

        var service = new Stripe.BillingPortal.SessionService();
        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = lastSubscription.CustomerId,
            ReturnUrl = referrer,
        };
        var session = await service.CreateAsync(options);
        var portalUrl = session.Url;

        return Ok(portalUrl);
    }

    [Authorize]
    [HttpGet("intent")]
    public async Task<IActionResult> CreateCheckoutSession()
    {
        var userId = GetUserId();
        logger.LogTrace($"Creating checkout session for user {userId}");

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

    [Authorize]
    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe()
    {
        var userId = GetUserId();
        logger.LogTrace($"Unsubscribing user {userId}");

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
        new SubscriptionService().Update(lastSubscription.Id, options);

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
                //case Events.CustomerSubscriptionCreated:
                //case Events.CustomerSubscriptionDeleted:
                //case Events.CustomerSubscriptionPaused:
                //case Events.CustomerSubscriptionPendingUpdateApplied:
                //case Events.CustomerSubscriptionPendingUpdateExpired:
                //case Events.CustomerSubscriptionTrialWillEnd:
                //case Events.CustomerSubscriptionResumed:
                case Events.CustomerSubscriptionUpdated:

                    if (stripeEvent.Data.Object is not Stripe.Subscription updatedSubscription)
                    {
                        return BadRequest("failed to cast event");
                    }
                    if (!updatedSubscription.Metadata.TryGetValue("UserId", out var updaterId))
                    {
                        return BadRequest("failed to get userid from metadata");
                    }
                    
                    var subscription = await new SubscriptionService().GetAsync(updatedSubscription.Id);
                    await subscriptionRepository.UpsertSubscription(
                        new Subscription()
                        {
                            Id = subscription!.Id,
                            UserId = updaterId!,
                            CustomerId = subscription.CustomerId,
                            Status = subscription.Status.ToSubscriptionStatus(),
                            ExpiresOn = subscription.CurrentPeriodEnd,
                            CreatedOn = stripeEvent.Created,
                            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd
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

