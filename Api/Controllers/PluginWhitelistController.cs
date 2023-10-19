using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiPlugin.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/pluginWhitelist")]
    public class PluginWhitelistController : ControllerBase
    {
        private readonly PluginRepository pluginRepository;
        private readonly PluginWhitelistedUserRepository pluginWhitelistedUserRepository;
        private readonly PluginWhitelistRepository pluginWhitelistRepository;
        private readonly IMapper mapper;

        public PluginWhitelistController(PluginRepository pluginRepository, PluginWhitelistedUserRepository pluginWhitelistedUserRepository, PluginWhitelistRepository pluginWhitelistRepository, IMapper mapper)
        {
            this.pluginRepository = pluginRepository;
            this.pluginWhitelistedUserRepository = pluginWhitelistedUserRepository;
            this.pluginWhitelistRepository = pluginWhitelistRepository;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Plugin>> AddToWhitelist([FromBody] PluginWhitelistCreateRequest request)
        {
            string userId = GetUserId();

            var createdRecord = await pluginWhitelistRepository.Add( new PluginWhitelist(request.Plugin.Id,
                request.WhitelistedUser.Email,request.Plugin,request.WhitelistedUser,false), GetUserId());

            return CreatedAtAction(nameof(AddToWhitelist),createdRecord);
        }

    }
}
