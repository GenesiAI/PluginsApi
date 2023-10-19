using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiPlugin.Api.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public ControllerBase()
        {
        }

        protected string GetUserFirebaseId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id != null) return id;
            throw new UnauthorizedAccessException("UserId not found");
        }

        protected bool IsMatchingAuthenticatedUserId(string userId)
        {
            return string.Equals(userId, GetUserFirebaseId(), StringComparison.OrdinalIgnoreCase);
        }
    }
}