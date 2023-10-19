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
        var isPremium = await subscriptionRepository.IsUserPremium(GetUserFirebaseId());
        User user = await userRepository.GetUser(GetUserFirebaseId());
        
        if (user is null || user.isDeleted)
        {
            throw new KeyNotFoundException($"User was not found");
        }
        
        return new UserInfo()
        {
            Id = user.Id,
            Email = user.Email,
            IsPremium = isPremium,
            ChatData = new ChatData(),
            CreatedAt = user.CreatedAt
        };
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateNewUser([FromBody] UserCreateRequest request)
    {
        // Get userId because, by this point, we should already have the info from the authentication method
        string userId = (GetUserFirebaseId());

        User user = await userRepository.GetUser(userId);

        if (user != null)
        {
            // If it's a user i already have but deleted. i'm gonna update the user to reuse the same record
            if (!user.isDeleted)
            {
                throw new Exception($"There is already a User with id {userId} in the system!");
            }
            else
            {
                user.isDeleted = false;
                user.CreatedAt = DateTime.UtcNow;

                await userRepository.UpdateUser(user);
            }
        }
        else
        {
            // Definition of new user
            user = new User()
            {
                Id = userId,
                Email = request.Email,
                isDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            user = await userRepository.AddNewUser(user);
        }


        return CreatedAtAction(nameof(CreateNewUser), new { userId = user.Id }, user);
    }

    [HttpDelete]
    public async Task<ActionResult<User>> DeleteUser()
    {
        // The delete functions doesn't really deleate a user but it activates the flag IsDeleted
        
        User user = await userRepository.GetUser(GetUserFirebaseId());

        if (user is null || user.isDeleted)
        {
            throw new KeyNotFoundException($"User was not found. It may have been already deleted.");
        }

        user.isDeleted = true;

        await userRepository.UpdateUser(user);

        return CreatedAtAction(nameof(DeleteUser), new { user.Id }, user);
    }
}