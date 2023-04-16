using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        [HttpPut]
        public Guid Create()
        {
            return new Guid();
            //todo save in db
        }
    }
}