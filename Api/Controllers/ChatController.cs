using System.Collections;
using AiPlugin.Domain.Plugin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Chat")]
public class ChatController : AiPlugin.Api.Controllers.ControllerBase
{
    private readonly ILogger<ChatController> logger;

    public ChatController(ILogger<ChatController> logger)
    {
        this.logger = logger;
    }

    [HttpPost()]
    public /*async Task<*/Chat/*>*/ PostMessage([FromBody] Chat chat)
    {
        //duplicte the last message but change the role to assistant
        chat.Messages = chat.Messages.Append(new Message { Role = "assistant", Content = chat.Messages.Last().Content });
        return chat;
    }

}

//todo move from here to dto/domain 
public class Chat
{
    public Plugin AiPlugin { get; set; } = null!;
    public IEnumerable<Message> Messages { get; set; } = null!;
}

public class Message
{
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
}