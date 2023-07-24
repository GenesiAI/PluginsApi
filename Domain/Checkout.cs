using System.ComponentModel.DataAnnotations;

public class Checkout
{
    [Key]
    public string CheckoutSessionId { get; set; }

    public string UserId { get; set; }

    public string Status { get; set; } // Consider using an Enum for Status if there are only a few fixed values
}
