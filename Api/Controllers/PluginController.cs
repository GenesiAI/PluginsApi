using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using AiPlugin.Domain.Manifest;
using AiPlugin.Domain;
using AiPlugin.Application.Old;
using AiPlugin.Application.Plugins;
using AiPlugin.Api.Dto;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PluginController : ControllerBase
    {
        private readonly IBaseRepository<Plugin> pluginRepository;

        private readonly int millisecondsDelay = 700;
        private readonly IMapper mapper;
        public PluginController(IBaseRepository<Plugin> pluginRepository, IMapper mapper)
        {
            this.pluginRepository = pluginRepository;
            this.mapper = mapper;
        }

        [HttpGet("{userId}/{pluginId}/.well-known/ai-plugin.json")]
        public async Task<ActionResult<AiPluginManifest>> GetManifest(Guid userId, Guid pluginId)
        {
            await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null)
            {
                return NotFound();
            }
            if (plugin.UserId != userId)
            {
                return BadRequest();
            }
            return Ok(mapper.Map<Plugin, AiPluginManifest>(plugin));
        }

        [HttpGet("{userId}/{pluginId}/openapi.json")]
        public async Task<ActionResult<OpenApiDocument>> GetOpenAPISpecification(Guid userId, Guid pluginId)
        {
            await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null)
            {
                return NotFound();
            }
            if (plugin.UserId != userId)
            {
                return BadRequest();
            }
            return Ok(mapper.Map<Plugin, OpenApiDocument>(plugin));
        }

        #region Plugin CRUD

        // Create plugin
        [HttpPost("{userId}/plugin")]
        public async Task<ActionResult<Plugin>> CreatePlugin(Guid userId, [FromBody] PluginCreateRequest request)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = mapper.Map<Plugin>(request);
            plugin.UserId = userId;
            var createdPlugin = await pluginRepository.Add(plugin);
            return CreatedAtAction(nameof(GetPlugin), new { userId, pluginId = createdPlugin.Id }, createdPlugin);
        }

        // Create section
        [HttpPost("{userId}/{pluginId}/section")]
        public async Task<ActionResult<Section>> CreateSection(Guid userId, Guid pluginId, [FromBody] SectionCreateRequest request)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();

            var section = mapper.Map<Section>(request);
            plugin.Sections = plugin.Sections?.Append(section);

            await pluginRepository.Update(plugin);
            return CreatedAtAction(nameof(GetAction), new { userId, pluginId, sectionId = section.Id }, section);
        }

        // Get plugins
        [HttpGet("{userId}/plugins")]
        public async Task<ActionResult<IEnumerable<Plugin>>> GetPlugins(Guid userId)
        {
            //await Task.Delay(millisecondsDelay);
            var plugins = await pluginRepository.Get().Where(p => p.UserId == userId).ToListAsync();
            foreach (var plugin in plugins)
            {
                plugin.Sections = plugin.Sections?.Where(s => s.isDeleted == false).ToList();
            }
            return Ok(plugins);
        }

        // Get plugin
        [HttpGet("{userId}/{pluginId}")]
        public async Task<ActionResult<Plugin>> GetPlugin(Guid userId, Guid pluginId)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();
            return Ok(plugin);
        }

        // Get section
        [HttpGet("{userId}/{pluginId}/{sectionId}")]
        public async Task<ActionResult<Section>> GetAction(Guid userId, Guid pluginId, Guid sectionId)
        {
            await Task.Delay(millisecondsDelay); //this one has to stay
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null)
            {
                return NotFound();
            }
            if (plugin.UserId != userId)
            {
                return BadRequest();
            }
            if (plugin.Sections == null)
            {
                return NotFound();
            }
            if (!plugin.Sections.Any(s => s.Id == sectionId))
            {
                return NotFound();
            }
            return Ok(plugin.Sections.Single(s => s.Id == sectionId));
            // return mapper.Map<Section, TextValue>(plugin);
        }

        // Update plugin
        [HttpPut("{userId}/{pluginId}")]
        public async Task<ActionResult> UpdatePlugin(Guid userId, Guid pluginId, [FromBody] PluginUpdateRequest request)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();

            mapper.Map(request, plugin);
            await pluginRepository.Update(plugin);

            return NoContent();
        }

        // Update section
        [HttpPut("{userId}/{pluginId}/{sectionId}")]
        public async Task<ActionResult> UpdateSection(Guid userId, Guid pluginId, Guid sectionId, [FromBody] SectionUpdateRequest request)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();

            var section = plugin.Sections?.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return NotFound();

            mapper.Map(request, section);
            await pluginRepository.Update(plugin);

            return NoContent();
        }

        // Delete plugin
        [HttpDelete("{userId}/{pluginId}")]
        public async Task<ActionResult> DeletePlugin(Guid userId, Guid pluginId)
        {
            //await Task.Delay(millisecondsDelay);
            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();

            await pluginRepository.Delete(plugin.Id);
            return NoContent();
        }

        // Delete section
        [HttpDelete("{userId}/{pluginId}/{sectionId}")]
        public async Task<ActionResult> DeleteSection(Guid userId, Guid pluginId, Guid sectionId)
        {
            //await Task.Delay(millisecondsDelay);

            var plugin = await pluginRepository.Get(pluginId);
            if (plugin == null || plugin.UserId != userId) return BadRequest();

            var section = plugin.Sections?.FirstOrDefault(s => s.Id == sectionId);
            if (section == null) return NotFound();
            plugin.Sections = plugin.Sections!.Select(s =>
            {
                if (s.Id == sectionId)
                {
                    s.isDeleted = true;
                }
                return s;
            });
            await pluginRepository.Update(plugin);

            return NoContent();
        }

        #endregion

        // [HttpPost("{userId}/text-to-plugin")]
        // public async Task<Plugin> CreateFromText(Guid userId, [FromBody] string content)
        // {
        //     await Task.Delay(millisecondsDelay);
        //     return await pluginRepository.CreatePlugin(userId, content);
        // }
    }
}