using AiPlugin.Api.Dto;
using AiPlugin.Domain.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public /*async Task<*/ChatResponse/*>*/ PostMessage([FromBody] Chat chat)
    {
        return new ChatResponse
        {
            Message = new Message { Role = "assistant", Content = chat.Messages.Last().Content },
            ChatData = new ChatData()
        };
    }
}
