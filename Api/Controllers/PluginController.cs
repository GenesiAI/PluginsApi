using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Domain.Manifest;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

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
    public async Task<ActionResult<AiPluginManifest>> GetManifest(Guid pluginId, [OpenApiParameterIgnore] Guid userId)
    {
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
    public async Task<ActionResult<OpenApiDocument>> GetOpenAPISpecification(Guid pluginId, [OpenApiParameterIgnore] Guid userId)
    {
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
    [Authorize]
    [HttpPost("plugin")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Plugin>> CreatePlugin([FromBody] PluginCreateRequest request, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = mapper.Map<Plugin>(request);
        plugin.UserId = userId;
        var createdPlugin = await pluginRepository.Add(plugin);
        return CreatedAtAction(nameof(GetPlugin), new { userId, pluginId = createdPlugin.Id }, createdPlugin);
    }

    // Create section
    [Authorize]
    [HttpPost("{pluginId}/section")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Section>> CreateSection(Guid pluginId, [FromBody] SectionCreateRequest request, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        var section = mapper.Map<Section>(request);
        plugin.Sections = plugin.Sections?.Append(section);

        await pluginRepository.Update(plugin);
        return CreatedAtAction(nameof(GetAction), new { userId, pluginId, sectionId = section.Id }, section);
    }

    //Get plugins
    [Authorize]
    [HttpGet("plugins")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<IEnumerable<Plugin>>> GetPlugins([OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugins = await pluginRepository.Get().Where(p => p.UserId == userId).ToListAsync();
        foreach (var plugin in plugins)
        {
            plugin.Sections = plugin.Sections?.Where(s => s.isDeleted == false).ToList();
        }
        return Ok(plugins);
    }

    // Get plugin
    [Authorize]
    [HttpGet("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Plugin>> GetPlugin(Guid pluginId, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();
        return Ok(plugin);
    }

    // Get section
    [Authorize]
    [HttpGet("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult<Section>> GetAction(Guid pluginId, Guid sectionId, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        await Task.Delay(millisecondsDelay); //in the future we might offer to pay to get faster responses
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
    [Authorize]
    [HttpPut("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> UpdatePlugin(Guid pluginId, [FromBody] PluginUpdateRequest request, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        mapper.Map(request, plugin);
        await pluginRepository.Update(plugin);

        return NoContent();
    }

    // Update section
    [Authorize]
    [HttpPut("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> UpdateSection(Guid pluginId, Guid sectionId, [FromBody] SectionUpdateRequest request, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        var section = plugin.Sections?.FirstOrDefault(s => s.Id == sectionId);
        if (section == null) return NotFound();

        mapper.Map(request, section);
        await pluginRepository.Update(plugin);

        return NoContent();
    }

    // Delete plugin
    [Authorize]
    [HttpDelete("{pluginId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> DeletePlugin(Guid pluginId, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

        var plugin = await pluginRepository.Get(pluginId);
        if (plugin == null || plugin.UserId != userId) return BadRequest();

        await pluginRepository.Delete(plugin.Id);
        return NoContent();
    }

    // Delete section
    [Authorize]
    [HttpDelete("{pluginId}/{sectionId}")]
    [UserIdFromSubdomain]
    public async Task<ActionResult> DeleteSection(Guid pluginId, Guid sectionId, [OpenApiParameterIgnore] Guid userId)
    {
        if (!DoesUserIdsMatch(userId)) return Unauthorized();

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

    private Guid? GetTokenUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(id, out Guid guidUserId) ? guidUserId : null;
    }

    private bool DoesUserIdsMatch(Guid userId)
    {
        return GetTokenUserId() == userId;
    }
}