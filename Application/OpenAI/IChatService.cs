using AiPlugin.Domain.Chat;

namespace AiPlugin.Application.OpenAI;

public interface IChatService
{
    public Task<Message> ReplyChatGPT(Chat rawChat);

}