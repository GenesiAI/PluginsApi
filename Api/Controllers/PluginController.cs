using System;
using System.Data;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiPlugin.Api.Dto;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain.Plugin;
using AutoMapper;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
    public async Task<AppAuth> SetupAuth(Guid pluginId)
    {
        var plugin = await pluginRepository.Get(pluginId);
        var app = await AddApp(plugin);
        return new AppAuth()
        {
            PluginId = pluginId,
            AppId = app.AppId,
            AppSecret = app.ApiKeyId
        };
    }

    private async Task<App> AddApp(Plugin plugin)
    {
        HttpClient client = await GetFirebaseAdminClient();
        var apps = await GetApps(client);

        var stringPluginId = plugin.Id.ToString();
        var appsThatMatchName = apps.Where(x => String.Compare(x.DisplayName, stringPluginId, true) == 0);
        if (appsThatMatchName.Any())
            throw new Exception($"App with name {stringPluginId} already exists");

        var newApp = new App()
        {
            DisplayName = stringPluginId
        };
        await CreateFirebaseApp(client, newApp);

        return await GetApp(client, stringPluginId) ?? throw new Exception($"App with name {stringPluginId} not found after creation");
    }

    private async Task CreateFirebaseApp(HttpClient client, App newApp)
    {
        var json = JsonSerializer.Serialize(new { displayName = newApp.DisplayName });
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync($"https://firebase.googleapis.com/v1beta1/projects/genesi-ai/webApps", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to create app: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

        var operation = await response.Content.ReadFromJsonAsync<Operation>();
        if (operation == null)
            throw new Exception($"Operation is null");

        await WaitOperationSuccess(client, operation.Name);
    }

    private static async Task<IEnumerable<App>> GetApps(HttpClient client)
    {
        HttpResponseMessage listResponse = await client.GetAsync("https://firebase.googleapis.com/v1beta1/projects/genesi-ai/webApps");
        var apps = await listResponse.Content.ReadFromJsonAsync<AppsList>();
        return apps?.Apps ?? new List<App>();
    }

    private static async Task<App?> GetApp(HttpClient client, string displayName)
    {
        HttpResponseMessage listResponse = await client.GetAsync("https://firebase.googleapis.com/v1beta1/projects/genesi-ai/webApps");
        var apps = await listResponse.Content.ReadFromJsonAsync<AppsList>();
        return apps?.Apps.SingleOrDefault(x => String.Compare(x.DisplayName, displayName, true) == 0);
    }

    private static async Task<HttpClient> GetFirebaseAdminClient()
    {
        var pathToServiceAccountKey = Path.Combine(Directory.GetCurrentDirectory(), "FirebaseDoNotShareKeys.json");
        GoogleCredential credential = GoogleCredential.FromFile(pathToServiceAccountKey)
            .CreateScoped("https://www.googleapis.com/auth/firebase");
        string token = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        Console.WriteLine(token);
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    // try to get positive or negative operation results for 1min from the operation
    private async Task WaitOperationSuccess(HttpClient client, string name)
    {
        //name is like  workflows/MDU5MTdhNjQtYjBiNi00MzJiLWI3MDQtMGYwNDBlOGZiZDY1"
        //target url GET https://firebase.googleapis.com/v1beta1/{name=operations/**}

        for (int i = 0; i < 60; i++)
        {
            HttpResponseMessage response = await client.GetAsync($"https://firebase.googleapis.com/v1beta1/{name}");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get operation: {response.StatusCode}");

            var operation = await response.Content.ReadFromJsonAsync<Operation>(
                );
            if (operation == null)
                throw new Exception($"Operation is null");

            if (operation.Done)
            {
                if (operation.Error != null)
                    throw new Exception($"Operation failed: {operation.Error}");
                return;
            }
            else
            {
                await Task.Delay(1000);
            }
        }
        throw new Exception($"Operation timeout");
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class AppsList
    {
        [JsonPropertyName("apps")]
        public List<App> Apps { get; set; }
    }

    public class App
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("appId")]
        public string AppId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("projectId")]
        public string ProjectId { get; set; }

        [JsonPropertyName("webId")]
        public string? WebId { get; set; }

        [JsonPropertyName("apiKeyId")]
        public string ApiKeyId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("expireTime")]
        public DateTime ExpireTime { get; set; }

        [JsonPropertyName("etag")]
        public string Etag { get; set; }
    }


    public class Operation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("metadata")]
        public object Metadata { get; set; } = null!;

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("error")]
        public object Error { get; set; } = null!;

        [JsonPropertyName("response")]
        public object Response { get; set; } = null!;

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

    public class AppAuth
    {
        public Guid PluginId { get; set; }
        public string AppId { get; set; } = null!;
        public string AppSecret { get; set; } = null!;
    }
}