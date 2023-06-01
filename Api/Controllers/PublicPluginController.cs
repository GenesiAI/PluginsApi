using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Domain.Manifest;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace AiPlugin.Api.Controllers;

//public stuffs
[ApiController]
public class PublicPluginController : ControllerBase
{
    private readonly IBaseRepository<Plugin> pluginRepository;
    private readonly int millisecondsDelay = 700;
    private readonly IMapper mapper;
    public PublicPluginController(IBaseRepository<Plugin> pluginRepository, IMapper mapper)
    {
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
    }

    [HttpGet(".well-known/ai-plugin.json")]
    [PlugindFromSubdomain]
    public async Task<ActionResult<AiPluginManifest>> GetManifest([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            return Ok(mapper.Map<Plugin, AiPluginManifest>(plugin));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("openapi.json")]
    [PlugindFromSubdomain]
    public async Task<ActionResult<OpenApiDocument>> GetOpenAPISpecification([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            return Ok(mapper.Map<Plugin, OpenApiDocument>(plugin));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{sectionName}")]
    [PlugindFromSubdomain]
    public async Task<ActionResult<Section>> GetSection(string sectionName, [OpenApiParameterIgnore] Guid pluginId)
    {
        //todo if the section require authenticated users check for authentication
        await Task.Delay(millisecondsDelay);
        Plugin plugin;
        try
        {
            plugin = await pluginRepository.Get(pluginId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        var section = plugin!.Sections?.SingleOrDefault(s => s.Name == sectionName);
        if (section?.isDeleted == false)
        {
            return NotFound();
        }
        return Ok(section);
        // return mapper.Map<Section, TextValue>(plugin); todo
    }
}