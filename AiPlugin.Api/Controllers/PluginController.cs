using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        [HttpGet("{userId}/.well-known/ai-plugin.json")]
        public async Task<string> Get(Guid userId)
        {
            await Task.Delay(4000);
            //gets the plugin doc from the db
            return """{"hello": "world"}""";
        }

        [HttpGet("{userId}/{actionId}")]
        public async Task<string> Get(Guid userId, Guid actionId)
        {
            await Task.Delay(4000);
            //gets the plugin action from the db
            return """{"hello": "world"}""";
        }

        [HttpPost("{userId}")]
        public async Task<string> Get(Guid userId, [FromBody] string content)
        {
            await Task.Delay(4000);
            return $$$"""
            {
                "userId":"{{{userId.ToString()}}}",
                "aiPlugin": "{{{content}}}"
            }
            """;
        }
    }
}