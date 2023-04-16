using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        [HttpGet("{userId}/.well-known/ai-plugin.json")]
        public string Get(Guid userId)
        {
            return "{hello: world}";
        }
    }
}