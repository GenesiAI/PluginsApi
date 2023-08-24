using System.ComponentModel.DataAnnotations;

public class Checkout
{
    [Key]
    public string CheckoutSessionId { get; set; } = null!;

    public string UserId { get; set; } = null!;

    public CheckoutStatuses Status { get; set; }
}

public enum CheckoutStatuses
{
    Pending,
    Success,
    Failed,
}