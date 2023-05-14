using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using AiPlugin.Domain;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Manifest;
using AiPlugin.Api.Dto;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Api.Controllers;

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

    [HttpGet("{pluginId}/.well-known/ai-plugin.json")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<AiPluginManifest>> GetManifest(Guid pluginId, Guid userId)
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

    [HttpGet("{pluginId}/openapi.json")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<OpenApiDocument>> GetOpenAPISpecification(Guid pluginId, Guid userId)
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
    [HttpPost("plugin")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Plugin>> CreatePlugin([FromBody] PluginCreateRequest request, Guid userId)
    {
        //await Task.Delay(millisecondsDelay);
        var plugin = mapper.Map<Plugin>(request);
        plugin.UserId = userId;
        var createdPlugin = await pluginRepository.Add(plugin);
        return CreatedAtAction(nameof(GetPlugin), new { userId, pluginId = createdPlugin.Id }, createdPlugin);
    }

    // Create section
    [HttpPost("{pluginId}/section")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Section>> CreateSection(Guid pluginId, [FromBody] SectionCreateRequest request, Guid userId)
    {
        //await Task.Delay(millisecondsDelay);
        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        var section = mapper.Map<Section>(request);
        plugin.Sections = plugin.Sections?.Append(section);

        await pluginRepository.Update(plugin);
        return CreatedAtAction(nameof(GetAction), new { userId, pluginId, sectionId = section.Id }, section);
    }

    //Get plugins
    [HttpGet("plugins")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<IEnumerable<Plugin>>> GetPlugins(Guid userId)
    {
        await Task.Delay(millisecondsDelay);
        var plugins = await pluginRepository.Get().Where(p => p.UserId == userId).ToListAsync();
        foreach (var plugin in plugins)
        {
            plugin.Sections = plugin.Sections?.Where(s => s.isDeleted == false).ToList();
        }
        return Ok(plugins);
    }

    // Get plugin
    [HttpGet("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Plugin>> GetPlugin(Guid pluginId, Guid userId)
    {
        //await Task.Delay(millisecondsDelay);
        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();
        return Ok(plugin);
    }

    // Get section
    [HttpGet("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Section>> GetAction(Guid pluginId, Guid sectionId, Guid userId)
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
    [HttpPut("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> UpdatePlugin(Guid pluginId, [FromBody] PluginUpdateRequest request, Guid userId)
    {
        //await Task.Delay(millisecondsDelay);
        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        mapper.Map(request, plugin);
        await pluginRepository.Update(plugin);

        return NoContent();
    }

    // Update section
    [HttpPut("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> UpdateSection(Guid pluginId, Guid sectionId, [FromBody] SectionUpdateRequest request, Guid userId)
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
    [HttpDelete("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> DeletePlugin(Guid pluginId, Guid userId)
    {
        //await Task.Delay(millisecondsDelay);
        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        await pluginRepository.Delete(plugin.Id);
        return NoContent();
    }

    // Delete section
    [HttpDelete("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> DeleteSection(Guid pluginId, Guid sectionId, Guid userId)
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

}