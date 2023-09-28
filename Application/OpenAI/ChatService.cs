using AiPlugin.Domain.Chat;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiPlugin.Application.OpenAI;

public class ChatService : IChatService
{
    private readonly GPTSettings gPTSettings;

    public ChatService(GPTSettings gPTSettings)
    {
        this.gPTSettings = gPTSettings;
    }
    public async Task<Message> ReplyChatGPT(Chat rawChat)
    {
        var api = new OpenAIAPI(gPTSettings.ApiKey);
        var chat = api.Chat.CreateConversation();


        foreach (var message in rawChat.Messages)
        {
            switch (message.Role)
            {
                case "user":
                    chat.AppendUserInput(message.Content);
                    break;

                case "assistant":
                    chat.AppendExampleChatbotOutput(message.Content);
                    break;

                default:
                    throw new InvalidOperationException("You can't do that");
                    break;
            }
        }

        //chat.AppendSystemMessage("You can request the usage of a function by typing ");

        return new Message() { Content = await chat.GetResponseFromChatbotAsync(), Role = ChatMessageRole.Assistant };
    }
}
