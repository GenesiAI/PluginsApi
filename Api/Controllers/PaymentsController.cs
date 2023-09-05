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

        // save row in checkouts
        // var checkout = new Checkout
        // {
        //     CheckoutSessionId = session.Id,
        //     UserId = userId,
        //     Status = CheckoutStatuses.Pending,
        // };
        // await subscriptionRepository.AddCheckout(checkout);

        // if GET was requested using flag ?automatic=false, return the session id; otherwise redirect to the checkout page
        if (Request.Query.ContainsKey("automatic") && Request.Query["automatic"] == "false")
        {
            return Ok(new { checkoutSessionId = session.Id });
        }
        return Ok(session.Url);
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
                //case Events.CheckoutSessionExpired:
                //    // if the checkout session has expired, mark the checkout as expired

                //    var checkout = await GetCheckoutFromEvent(stripeEvent);
                //    if (checkout == null)
                //    {
                //        return BadRequest();
                //    }
                //    checkout.Status = CheckoutStatuses.Failed;
                //    await subscriptionRepository.UpdateCheckout(checkout);
                //    return Ok();    // return a 200 response so that Stripe doesn't retry the webhook

                //case Events.CheckoutSessionCompleted:

                //    var completedCheckout = await GetCheckoutFromEvent(stripeEvent);
                //    if (completedCheckout == null)
                //    {
                //        return BadRequest();
                //    }
                //    completedCheckout.Status = CheckoutStatuses.Success;
                //    await subscriptionRepository.UpdateCheckout(completedCheckout);
                //    return Ok();

                case Events.CustomerSubscriptionCreated:
                case Events.CustomerSubscriptionDeleted:
                case Events.CustomerSubscriptionPaused:
                case Events.CustomerSubscriptionPendingUpdateApplied:
                case Events.CustomerSubscriptionPendingUpdateExpired:
                case Events.CustomerSubscriptionTrialWillEnd:
                case Events.CustomerSubscriptionResumed:
                case Events.CustomerSubscriptionUpdated:

                    if (stripeEvent.Data.Object is not Stripe.Subscription subscription)
                    {
                        return BadRequest("failed to cast event");
                    }
                    if (!subscription.Metadata.TryGetValue("UserId", out var userid))
                    {
                        return BadRequest("failed to get userid from metadata");
                    }

                    await subscriptionRepository.AddSubscription(
                         new Subscription()
                         {
                             SubscriptionId = subscription!.Id,
                             UserId = userid!,
                             CustomerId = subscription.CustomerId,
                             Status = subscription.Status.ToSubscriptionStatus(),
                             ExpiresOn = subscription.CurrentPeriodEnd,
                             CreatedOn = stripeEvent.Created //todo unix time, need to convert to uct?
                         }
                     );
                    //var subscriptionToUpdate = await subscriptionRepository.GetSubscription(subscription.Id);
                    //if (subscriptionToUpdate == null)
                    //{
                    //    return BadRequest();
                    //}
                    //subscriptionToUpdate.Status = subscription.Subscription.Status.ToSubscriptionStatus();
                    //await subscriptionRepository.UpdateSubscription(subscriptionToUpdate);
                    return Ok();

                default:
                    logger.LogWarning("Unhandled event type: {0}", stripeEvent.Type);
                    return Ok();
            }
        }
        catch (StripeException stripeException)
        {
            logger.LogCritical(stripeException, "Exception while trying to handle stripe event");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Exception while trying to handle stripe event");
            throw;
        }
    }
    // private async Task<Checkout?> GetCheckoutFromEvent(Event stripeEvent)
    // {
    //     var session = stripeEvent.Data.Object as Session;
    //     if (session?.Id == null)
    //     {
    //         logger.LogError("CheckoutSessionExpired: sessionToDelete.Id is null");
    //         return null;
    //     }
    //     var checkout = await subscriptionRepository.GetCheckout(session.Id);
    //     if (checkout == null)
    //     {
    //         logger.LogError($"CheckoutSessionExpired: checkoutToDelete is null for session {session.Id}");
    //         return null;
    //     }
    //     return checkout;
    // }
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

