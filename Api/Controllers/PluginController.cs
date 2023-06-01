using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plugins")]
public class PluginController : ControllerBase
{
    private readonly IBaseRepository<Plugin> pluginRepository;
    private readonly IMapper mapper;
    public PluginController(IBaseRepository<Plugin> pluginRepository, IMapper mapper)
    {
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
    }

    // Create plugin
    [HttpPost]
    public async Task<ActionResult<Plugin>> CreatePlugin([FromBody] PluginCreateRequest request)
    {
        var plugin = mapper.Map<Plugin>(request);
        plugin.UserId = GetUserId();
        var createdPlugin = await pluginRepository.Add(plugin);
        return CreatedAtAction(nameof(CreatePlugin), new { userId = createdPlugin.UserId, pluginId = createdPlugin.Id }, createdPlugin);
    }

    // Get plugins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Plugin>>> GetPlugins()
    {
        var userId = GetUserId();
        var plugins = await pluginRepository.Get().Where(p => p.UserId == userId).ToListAsync();
        return Ok(plugins);
    }

    // Get plugin
    [HttpGet("{pluginId}")]
    public async Task<ActionResult<Plugin>> GetPlugin(Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            return Ok(plugin);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Update plugin
    [HttpPut("{pluginId}")]
    public async Task<ActionResult> UpdatePlugin(PluginUpdateRequest request, Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to update this plugin");
            plugin = mapper.Map(request, plugin);
            await pluginRepository.Update(plugin);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

    }

    // Delete plugin
    [HttpDelete("{pluginId}")]
    public async Task<ActionResult> DeletePlugin(Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to delete this plugin");
            await pluginRepository.Delete(pluginId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


    // Create section
    [HttpPost("{pluginId}/sections")]
    public async Task<ActionResult<Section>> CreateSection(SectionCreateRequest request, Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to create a section for this plugin");
            var section = mapper.Map<Section>(request);
            section.PluginId = pluginId;
            plugin.Sections = plugin.Sections == null ? new List<Section>() { section } : plugin.Sections.Append(section);
            var createdSection = await pluginRepository.Update(plugin);
            return CreatedAtAction(nameof(CreateSection), new { userId = plugin.UserId, pluginId = plugin.Id, sectionId = createdSection.Id }, createdSection);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Get sections
    [HttpGet("{pluginId}/sections")]
    public async Task<ActionResult<IEnumerable<Section>>> GetSections(Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to get sections for this plugin");
            return Ok(plugin.Sections?.Where(s => s.Id == pluginId) ?? new List<Section>());
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Get section
    [HttpGet("{pluginId}/sections/{sectionId}")]
    public async Task<ActionResult<Section>> GetSection(Guid pluginId, Guid sectionId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to get sections for this plugin");
            var section = plugin!.Sections?.SingleOrDefault(s => s.Id == sectionId);
            if (section?.isDeleted == false)
            {
                return NotFound();
            }
            return Ok(plugin.Sections?.Where(s => s.Id == pluginId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Update section
    [HttpPut("{pluginId}/sections/{sectionId}")]
    public async Task<ActionResult> UpdateSection(Guid sectionId, [FromBody] SectionUpdateRequest request, Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to update this section");
            var section = plugin.Sections?.SingleOrDefault(s => s.Id == sectionId);
            if (plugin.Sections == null || section == null) return NotFound("Section not found");
            mapper.Map(request, section);
            await pluginRepository.Update(plugin);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // Delete section
    [HttpDelete("{pluginId}/sections/{sectionId}")]
    public async Task<ActionResult> DeleteSection(Guid sectionId, Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!IsMatchingAuthenticatedUserId(plugin.UserId)) return Unauthorized("User is not authorized to delete this section");
            var section = plugin.Sections?.SingleOrDefault(s => s.Id == sectionId);
            if (plugin.Sections == null || section == null) return NotFound("Section not found");
            section.isDeleted = true;
            await pluginRepository.Update(plugin);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    #region private methods
    private string GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (id != null) return id;
        throw new UnauthorizedAccessException("UserId not found");
    }
    private bool IsMatchingAuthenticatedUserId(string userId)
    {
        return string.Equals(userId, GetUserId(), StringComparison.OrdinalIgnoreCase);
    }
    #endregion
}