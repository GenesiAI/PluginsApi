using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Common.Manifest;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Microsoft.EntityFrameworkCore.Design;
using AiPlugin.Api.Dto;
using AiPlugin.Api.Settings;

namespace AiPlugin.Api.Controllers;

//public stuffs
[ApiController]
public class PublicPluginController : Controller
{
    private readonly int millisecondsDelay = 700;
    private readonly SubscriptionRepository subscriptionRepository;
    private readonly IPluginRepository pluginRepository;
    private readonly IMapper mapper;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ContactSetting contactSettings;

    public PublicPluginController(
        IPluginRepository pluginRepository,
        SubscriptionRepository subscriptionRepository,
        IMapper mapper,
        IHttpClientFactory httpClientFactory,
        ContactSetting contactSettings)
    {
        this.subscriptionRepository = subscriptionRepository;
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
        this.httpClientFactory = httpClientFactory;
        this.contactSettings = contactSettings;
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

    /// <summary>
    /// Echoes the the contact request to the service that handles it hinding the key, basically proxy.
    /// </summary>

    [HttpPost("contact")]
    public async Task<IActionResult> Contact(ContactFormRequest contactRequest)
    {
        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync(contactSettings.Url, contactRequest);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException("Error contacting the service " + response.StatusCode + " " + response.ReasonPhrase);
        }
        return Ok();
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
            if (section?.isDeleted != false)
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