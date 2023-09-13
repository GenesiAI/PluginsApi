using Microsoft.AspNetCore.Mvc;
using AiPlugin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using AiPlugin.Api.Dto;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly SubscriptionRepository subscriptionRepository;

    public UserController(SubscriptionRepository subscriptionRepository)
    {
        this.subscriptionRepository = subscriptionRepository;
    }

    [HttpGet]
    public async Task<UserInfo> GetUserInfo()
    {

        var isPremium = await subscriptionRepository.IsUserPremium(GetUserId());
        return new UserInfo() { IsPremium = isPremium , ChatData = new ChatData()};
    }
}