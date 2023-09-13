using OpenAI_API.Chat;

namespace AiPlugin.Domain.Chat;

public class Message
{
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
}