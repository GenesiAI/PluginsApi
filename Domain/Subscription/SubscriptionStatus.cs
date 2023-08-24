
/// <summary>
/// Possible values are incomplete, incomplete_expired, trialing, active, past_due,
/// canceled, or unpaid. For collection_method=charge_automatically a subscription
/// moves into incomplete if the initial payment attempt fails. A subscription in
/// this state can only have metadata and default_source updated. Once the first
/// invoice is paid, the subscription moves into an active state. If the first invoice
/// is not paid within 23 hours, the subscription transitions to incomplete_expired.
/// This is a terminal state, the open invoice will be voided and no further invoices
/// will be generated. A subscription that is currently in a trial period is trialing
/// and moves to active when the trial period is over. If subscription collection_method=charge_automatically,
/// it becomes past_due when payment is required but cannot be paid (due to failed
/// payment or awaiting additional user actions). Once Stripe has exhausted all payment
/// retry attempts, the subscription will become canceled or unpaid (depending on
/// your subscriptions settings). If subscription collection_method=send_invoice
/// it becomes past_due when its invoice is not paid by the due date, and canceled
/// or unpaid if it is still not paid by an additional deadline after that. Note
/// that when a subscription has a status of unpaid, no subsequent invoices will
/// be attempted (invoices will be created, but then immediately automatically closed).
/// After receiving updated payment information from a customer, you may choose to
/// reopen and pay their closed invoices. One of: active, canceled, incomplete, incomplete_expired,
/// past_due, paused, trialing, or unpaid.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// a subscription moves into incomplete if the initial payment attempt fails
    /// </summary>
    Incomplete,
    /// <summary>
    /// a subscription moves into incomplete_expired if the initial payment attempt fails and the subscription is not paid within 23 hours
    /// </summary>
    IncompleteExpired,
    /// <summary>
    /// a subscription that is currently in a trial period is trialing and moves to active when the trial period is over
    /// </summary>
    Trialing,
    /// <summary>
    /// a subscription moves into active when the first invoice is paid
    /// </summary>
    Active,
    /// <summary>
    /// a subscription becomes past_due when payment is required but cannot be paid (due to failed payment or awaiting additional user actions)
    /// </summary>
    PastDue,

    /// <summary>
    /// a subscription becomes canceled when Stripe has exhausted all payment retry attempts.         
    /// </summary>
    Canceled,

    /// <summary>
    /// a subscription becomes unpaid when its invoice is not paid by the due date
    /// </summary>
    Unpaid
}

public static class StringExtensions
{
    /// <summary>
    /// Converts a string to the corresponding SubscriptionStatus enum value.
    /// </summary>
    /// <param name="status">The string representation of the SubscriptionStatus.</param>
    /// <returns>The SubscriptionStatus enum value.</returns>
    /// <exception cref="ArgumentException">Thrown when the string does not match any SubscriptionStatus enum value.</exception>
    public static SubscriptionStatus ToSubscriptionStatus(this string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            throw new ArgumentException("Status cannot be null or empty.", nameof(status));
        }

        if (Enum.TryParse(typeof(SubscriptionStatus), status, true, out var result))
        {
            return (SubscriptionStatus)result;
        }

        throw new ArgumentException($"'{status}' is not a valid SubscriptionStatus value.", nameof(status));
    }
}
