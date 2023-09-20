namespace AiPlugin.Api.Dto.User;

/// <summary>
/// Object used to save User id, e-mail and various status flags
/// </summary>
public class UserInfo
{
    /// <summary>
    /// UserId that we get from the authentication method
    /// </summary>
    public string UserId { get; set; }
    /// <summary>
    /// The email the user used to sign up
    /// </summary>
    public string Email { get; set; }
    public int FirebaseId { get; set; }
    /// <summary>
    /// Flag indicating if the user is allowed to use the premium features
    /// </summary>
    public bool IsPremium { get; set; }
    /// <summary>
    /// When the user signed up
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public ChatData ChatData { get; set; } = null!;
}

//todo move from here to dto/domain
public class ChatData
{
    public int MaxMessagesLast24H { get; set; }
    public int MessagesLast24H { get; set; }
}
