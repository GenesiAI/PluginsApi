// this class is the base controller for all controllers, gives access to the user id
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthBase.Controllers
{
    public class AuthController : Controller
    {

        [Authorize]
        protected string GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id != null) return id;
            throw new UnauthorizedAccessException("UserId not found");
        }

        [Authorize]
        protected bool IsMatchingAuthenticatedUserId(string userId)
        {
            return string.Equals(userId, GetUserId(), StringComparison.OrdinalIgnoreCase);
        }
    }
}