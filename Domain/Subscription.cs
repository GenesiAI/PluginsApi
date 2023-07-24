using System.ComponentModel.DataAnnotations;
public class Subscription
{
    [Key]
    public string SubscriptionId { get; set; }

    public string UserId { get; set; }

    public string Status { get; set; } // Consider using an Enum for Status if there are only a few fixed values

    public DateTime ExpiresOn { get; set; }

    public string CustomerId { get; set; }

    public DateTime EventTime { get; set; }
}
