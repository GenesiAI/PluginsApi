using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Domain.Manifest;
using AutoMapper;
using Utilities.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace AiPlugin.Api.Controllers;

//public stuffs
[ApiController]
public class PublicPluginController : ControllerBase
{
    private readonly IPluginRepository pluginRepository;
    private readonly int millisecondsDelay = 700;
    private readonly IMapper mapper;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ContactSetting contactSettings;

    public PublicPluginController(IPluginRepository pluginRepository, IMapper mapper, IHttpClientFactory httpClientFactory, ContactSetting contactSettings)
    {
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
        this.httpClientFactory = httpClientFactory;
        this.contactSettings = contactSettings;
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
    public async Task<IActionResult> GetOpenAPISpecification([OpenApiParameterIgnore] Guid pluginId)
    {
        try
        {
            var plugin = await pluginRepository.Get(pluginId);
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
            return BadRequest();
        }
        return Ok();
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
        if (section?.isDeleted != false)
        {
            return NotFound();
        }
        return Ok(section);
        // return mapper.Map<Section, TextValue>(plugin); todo
    }
}