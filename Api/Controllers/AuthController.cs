// this class is the base controller for all controllers, gives access to the user id
using System.Security.Claims;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AuthBase.Controllers
{
    public class AuthController : Controller
    {
        protected readonly SubscriptionRepository _subscriptionRepository;
        protected readonly IBaseRepository<Plugin> pluginRepository;
        protected readonly IMapper mapper;

        // Default constructor without pluginRepository and mapper parameters
        public AuthController(SubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        // Overloaded constructor with pluginRepository and mapper parameters
        public AuthController(SubscriptionRepository subscriptionRepository, IBaseRepository<Plugin> pluginRepository, IMapper mapper)
            : this(subscriptionRepository) // Call the default constructor to set the subscriptionRepository parameter
        {
            this.pluginRepository = pluginRepository;
            this.mapper = mapper;
        }
        
        protected bool userHasActiveSubscription()
        {
            var userId = GetUserId();
            return userHasActiveSubscription(userId);
        }

        protected bool userHasActiveSubscription(string userId)
        {
            var subscriptions = _subscriptionRepository.GetSubscriptionByUserId(userId).Result;
            return subscriptions != null && subscriptions.ExpiresOn > DateTime.UtcNow;
        }
        
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