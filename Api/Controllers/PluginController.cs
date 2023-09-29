using System.Net;
using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiPlugin.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/plugins")]
public class PluginController : ControllerBase
{
    private readonly SubscriptionRepository subscriptionRepository;
    private readonly IPluginRepository pluginRepository;
    private readonly IMapper mapper;

    public PluginController(SubscriptionRepository subscriptionRepository, IPluginRepository pluginRepository, IMapper mapper)
        : base()
    {
        this.subscriptionRepository = subscriptionRepository;
        this.pluginRepository = pluginRepository;
        this.mapper = mapper;
    }

    [HttpGet("auth/setup/{pluginId}")]
    public async Task SetupAuth(Guid pluginId)
    {
        var plugin = await pluginRepository.Get(pluginId);
        var pathToServiceAccountKey = "C:/Users/cesca/repos/_secrets/genesi-ai-3fae60b65a57.json";
        GoogleCredential credential = GoogleCredential.FromFile(pathToServiceAccountKey)
            .CreateScoped("https://www.googleapis.com/auth/firebase");
        string token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        Console.WriteLine(token);

        string projectId = "genesi-ai";

        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Fetch existing web apps
        HttpResponseMessage listResponse = await client.GetAsync($"https://firebase.googleapis.com/v1beta1/projects/{projectId}/webApps");
        string listResponseBody = await listResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"List response: {listResponseBody}");

        // Parse JSON to check if app with displayName = pluginId.ToString() exists
        dynamic apps = Newtonsoft.Json.JsonConvert.DeserializeObject(listResponseBody);
        Console.WriteLine($"Apps: {apps}");
        bool appExists = false;

        foreach (var app in apps.apps)
        {
            if (app.displayName == pluginId.ToString())
            {
                appExists = true;
                break;
            }
        }

        if (appExists)
        {
            Console.WriteLine($"App with displayName {pluginId} already exists.");
            return;
        }
            
        var payload = new
        {
            displayName = pluginId.ToString(),
        };

        StringContent content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(payload), System.Text.Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync($"https://firebase.googleapis.com/v1beta1/projects/{projectId}/webApps", content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Success: {responseBody}");
        }
        else
        {
            Console.WriteLine($"Failed: {response.StatusCode}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Plugin>> CreatePlugin([FromBody] PluginCreateRequest request)
    {
        string userId = GetUserId();

        if (!await pluginRepository.HasReachedPluginQuota(userId))
            return BadRequest("Max plugins reached");

        var plugin = mapper.Map<Plugin>(request);
        plugin.UserId = userId;
        var createdPlugin = await pluginRepository.Add(plugin, userId);

        return CreatedAtAction(nameof(CreatePlugin), new { userId = createdPlugin.UserId, pluginId = createdPlugin.Id }, createdPlugin);
    }

    [HttpGet]
    public async Task<ActionResult<PluginsResponse>> GetPlugins()
    {
        string userId = GetUserId();
        var plugins = await pluginRepository.GetByUserId(userId);

        var result = mapper.Map<PluginsResponse>(plugins);
        result.MaxPlugins = await subscriptionRepository.IsUserPremium(userId) ? 10 : 3;
        return Ok(result);
    }

    // Get plugin
    [HttpGet("{pluginId}")]
    public async Task<ActionResult<Plugin>> GetPlugin(Guid pluginId)
    {
        try
        {
            return Ok(await pluginRepository.Get(pluginId));
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
}