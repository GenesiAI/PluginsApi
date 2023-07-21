public class CreateSubscriptionRequest
{
    // public string UserId { get; set; }  NOTE: This is not needed because we are using the authenticated user's ID! Only problem is that GetUserId() is private to the PluginController
    public string PlanId { get; set; }
}
