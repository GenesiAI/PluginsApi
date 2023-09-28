using AiPlugin.Api.Dto;
using AiPlugin.Application.OpenAI;
using AiPlugin.Domain.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/Chat")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> logger;
    private readonly IChatService chatService;
    private readonly SubscriptionRepository subscriptionRepository;

    public ChatController(ILogger<ChatController> logger, IChatService chatService, SubscriptionRepository subscriptionRepository)
    {
        this.logger = logger;
        this.chatService = chatService;
        this.subscriptionRepository = subscriptionRepository;
    }

    [HttpPost()]
    public async Task<ChatResponse> PostMessage([FromBody] TestChat chat)
    {
        var isUserPremium = await subscriptionRepository.IsUserPremium(GetUserId());
        // todo check if user has reached max messages
        return new ChatResponse
        {
            Message = await chatService.ReplyChatGPT(chat),
            ChatData = new ChatData()
            {

                MaxMessagesLast24H = isUserPremium ? 100 : 10,
                MessagesLast24H = 0 //todo featch
            }
        };
    }
}
