public class AdminWhitelist
{
    public List<string>? emails { get; set; }
    public bool Contains(string email)
    {
        if (email == null)
        {
            return false;
        }
        return this.emails != null && this.emails.Any(e => string.Equals(e, email, StringComparison.OrdinalIgnoreCase));
    }
}
