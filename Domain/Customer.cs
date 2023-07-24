using System.ComponentModel.DataAnnotations;
public class Customer
{
    [Key]
    public string UserId { get; set; }

    public string CustomerId { get; set; }
}
