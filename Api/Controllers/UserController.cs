using AiPlugin.Api.Dto.User;
using AiPlugin.Application.Users;
using AiPlugin.Domain.User;
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
        User user = await userRepository.GetUser(GetUserId());
        return new UserInfo()
        {
            Email = user.Email,
            FirebaseId = user.FirebaseId,
            IsPremium = isPremium,
            ChatData = new ChatData(),
            CreatedAt = user.CreatedAt
        };
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateNewUser([FromBody] UserCreateRequest request)
    {
        // Get userId because, by this point, we should already have the info from the authentication method
        string userId = GetUserId();
        if (await userRepository.GetUser(userId) != null)
        {
            throw new Exception($"There is already a User with id {userId} in the system!");
        }

        // Definition of new user
        User user = new User()
        {
            Id = new Guid(),
            UserId = userId,
            Email = request.Email,
            FirebaseId = request.FirebaseId,
            isDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        User createdUser = await userRepository.AddNewUser(user);

        return CreatedAtAction(nameof(CreateNewUser), new { userId = createdUser.UserId }, createdUser);
    }
}