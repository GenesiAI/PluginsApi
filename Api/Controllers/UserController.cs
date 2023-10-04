using AiPlugin.Api.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        DateTime? expireDate = await subscriptionRepository.GetExpireDate(GetUserId());
        bool? isAutoRenewActive = await subscriptionRepository.IsAutoRenewActive(GetUserId());
        return new UserInfo() {
            IsPremium = isPremium,
            ExpireDate = expireDate,
            IsAutoRenewActive = isAutoRenewActive,
            ChatData = new ChatData()
        };
    }
}