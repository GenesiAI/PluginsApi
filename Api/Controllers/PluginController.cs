using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using AiPlugin.Application;
using AiPlugin.Domain.Manifest;
using AiPlugin.Domain;

namespace AiPlugin.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        private readonly IPluginRepository pluginRepository;
        private readonly int millisecondsDelay = 2000;
        private readonly IMapper mapper;
        public PluginController(IPluginRepository pluginRepository, IMapper mapper)
        {
            this.pluginRepository = pluginRepository;
            this.mapper = mapper;
        }

        [HttpGet("{userId}/{pluginId}/.well-known/ai-plugin.json")]
        public async Task<AiPluginManifest> GetManifest(Guid userId, Guid pluginId)
        {
            await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.GetPlugin(userId, pluginId);
            return mapper.Map<Plugin, AiPluginManifest>(plugin);
        }


        [HttpGet("{userId}/{pluginId}/openapi.json")]
        public async Task<OpenApiDocument> GetOpenAPISpecification(Guid userId, Guid pluginId)
        {
            await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.GetPlugin(userId, pluginId);
            return mapper.Map<Plugin, OpenApiDocument>(plugin);
        }

        [HttpGet("{userId}/{pluginId}/{sectionId}")]
        public async Task<Section> GetAction(Guid userId, Guid pluginId, Guid sectionId)
        {
            await Task.Delay(millisecondsDelay);
            return await pluginRepository.GetSection(userId, pluginId, sectionId);
            // return mapper.Map<Section, TextValue>(plugin);
        }

        [HttpPost("{userId}/text-to-plugin")]
        public async Task<Plugin> CreateFromText(Guid userId, [FromBody] string content)
        {
            await Task.Delay(millisecondsDelay);
            return await pluginRepository.CreatePlugin(userId, content);
        }
    }
}