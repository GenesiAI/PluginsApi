namespace AiPlugin.Api.Dto;

public class UserInfo
{
    public bool IsPremium { get; set; }
    public ChatData ChatData { get; set; } = null!;
}

//todo move from here to dto/domain
public class ChatData
{
    public int MaxMessagesLast24H { get; set; }
    public int MessagesLast24H { get; set; }
}
