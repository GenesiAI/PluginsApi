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
// Q: write a chat object in json
// A: {"aiPlugin":{"id":"1","name":"test","description":"test","version":"1.0.0","author":"test","url":"http://localhost:5000","icon":"test","enabled":true},"messages":[{"role":"user","content":"hello"},{"role":"assistant","content":"hello"}]}
// Q: id is a guid
// A: 

