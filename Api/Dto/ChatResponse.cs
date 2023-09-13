using AiPlugin.Domain.Chat;

namespace AiPlugin.Api.Dto;
public class ChatResponse
{
    public Message Message { get; set; } = null!;
    public ChatData ChatData { get; set; } = null!;
}