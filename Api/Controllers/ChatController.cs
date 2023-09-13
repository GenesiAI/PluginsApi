using AiPlugin.Api.Dto;
using AiPlugin.Application.OpenAI;
using AiPlugin.Domain.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Chat")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> logger;
    private readonly IChatService chatService;

    public ChatController(ILogger<ChatController> logger, IChatService chatService)
    {
        this.logger = logger;
        this.chatService = chatService;
    }

    [HttpPost()]
    public async Task<ChatResponse> PostMessage([FromBody] Chat chat)
    {
        return new ChatResponse
        {
            Message = await chatService.ReplyChatGPT(chat),
            ChatData = new ChatData()
        };
    }
}
