using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Domain.Manifest;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace AiPlugin.Api.Controllers;

//public stuffs
[ApiController]
public class PublicPluginController : AuthBase.Controllers.AuthController
{
    private readonly int millisecondsDelay = 700;
    public PublicPluginController(SubscriptionRepository subscriptionRepository, IBaseRepository<Plugin> pluginRepository, IMapper mapper)
        : base(subscriptionRepository, pluginRepository, mapper)
    {
    }

    [HttpGet(".well-known/ai-plugin.json")]
    [PluginIdFromSubdomain]
    public async Task<ActionResult<AiPluginManifest>> GetManifest([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            AppPlugin appPlugin = GetPlugin(pluginId);
            if ( ! appPlugin.IsActive ) return Unauthorized();

            Plugin plugin = mapper.Map<Plugin>(appPlugin);
            
            return Ok(mapper.Map<Plugin, AiPluginManifest>(plugin));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("openapi.json")]
    [PluginIdFromSubdomain]
    public async Task<IActionResult> GetOpenAPISpecification([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            AppPlugin appPlugin = GetPlugin(pluginId);
            if ( ! appPlugin.IsActive ) return Unauthorized();
            
            Plugin plugin = mapper.Map<Plugin>(appPlugin);
            var result = mapper.Map<Plugin, OpenApiDocument>(plugin);

            using (var writer = new StringWriter())
            {
                result.SerializeAsV3(new OpenApiJsonWriter(writer));

                return new ContentResult
                {
                    Content = writer.ToString(),
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{sectionName}")]
    [PluginIdFromSubdomain]
    public async Task<ActionResult<Section>> GetSection(string sectionName, [OpenApiParameterIgnore] Guid pluginId)
    {
        Plugin plugin;
        try
        {
            AppPlugin appPlugin = GetPlugin(pluginId);
            if (!appPlugin.IsActive) return Unauthorized();

            plugin = mapper.Map<Plugin>(appPlugin);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        
        if ( ! userHasActiveSubscription(plugin.UserId) )
        {
            await Task.Delay(millisecondsDelay);
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