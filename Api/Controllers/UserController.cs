using AiPlugin.Api.Dto;
using AiPlugin.Api.Dto.User;
using AiPlugin.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserRepository userRepository;
    private readonly SubscriptionRepository subscriptionRepository;

    public UserController(UserRepository userRepository, SubscriptionRepository subscriptionRepository)
    {
        this.userRepository = userRepository;
        this.subscriptionRepository = subscriptionRepository;
    }

    [HttpGet]
    public async Task<UserInfo> GetUserInfo()
    {
        var isPremium = await subscriptionRepository.IsUserPremium(GetUserId());
        return new UserInfo() { IsPremium = isPremium, ChatData = new ChatData() };
    }

    [HttpPost]
    public async Task<UserInfo> CreateNewUser([FromBody] UserCreateRequest request)
    {
        // Get userId because, by this point, we should already have the info from the authentication method
        string userId = GetUserId();

        //if()

        return new UserInfo();
    }
}