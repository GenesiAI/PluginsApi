using System.ComponentModel.DataAnnotations;
using AiPlugin.Domain.Common;

public partial class Subscription 
{
    public string Id { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public string CustomerId { get; set; } = null!;

    public SubscriptionStatus Status { get; set; }

    public DateTime ExpiresOn { get; set; }

    public DateTime CreatedOn { get; set; }
}
