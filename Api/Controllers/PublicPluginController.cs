using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Common.Manifest;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace AiPlugin.Api.Controllers;

//public stuffs
[ApiController]
public class PublicPluginController : Controller
{
    private readonly int millisecondsDelay = 700;
    private readonly SubscriptionRepository subscriptionRepository;
    private readonly IPluginRepository pluginRepository;
    private readonly IMapper mapper;

    public PublicPluginController(SubscriptionRepository subscriptionRepository, IPluginRepository pluginRepository, IMapper mapper)
        : base()
    {
        this.subscriptionRepository = subscriptionRepository;
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
    }

    [HttpGet(".well-known/ai-plugin.json")]
    [PluginIdFromSubdomain]
    public async Task<ActionResult<AiPluginManifest>> GetManifest([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!plugin.IsActive) return NotFound();
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
            var plugin = await pluginRepository.Get(pluginId);
            if (!plugin.IsActive) return NotFound();

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
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
            if (!plugin.IsActive) return NotFound();

            if (!await subscriptionRepository.IsUserPremium(plugin.UserId))
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
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}