using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Domain.Manifest;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json.Linq;

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

    /**
    * Receives a POST request containing an email, name, and message as input.
    * Validates the email using a standard regular expression (regex).
    * Sends an HTTP POST request to the specified endpoint with the message payload.
    * This endpoint does not require user authentication.
    * Returns a 200 OK status code if the message is successfully sent.
    * Returns a 400 Bad Request status code if the input data is missing, or the email is invalid.
    *
    * @param message The JSON object containing the email, name, and message.
    * @returns IActionResult indicating the status of the request.
    */
    [HttpPost("contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Contact([FromBody] JObject message)
    {
        if (message == null)
        {
            return BadRequest();
        }

        var email = message.Value<string>("email");
        var name = message.Value<string>("name");
        var msg = message.Value<string>("message");

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(msg))
        {
            return BadRequest();
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
        {
            return BadRequest();
        }

        string signature = Environment.GetEnvironmentVariable("AZURE_LOGIC_CONTACT_EMAIL_SIGNATURE");

        if (string.IsNullOrEmpty(signature))
        {
            // Handle the case where the environment variable is not set
            return BadRequest();
        }

        var newMessage = new JObject {
            ["email"] = email,
            ["name"] = name,
            ["message"] = msg
        };

        using (var client = new HttpClient())
        {
            var content = new StringContent(newMessage.ToString(), System.Text.Encoding.UTF8, "application/json");
            var url = "https://prod-250.westeurope.logic.azure.com:443/workflows/884b292e648b4b26beeed8d79e2341cc/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=" + signature;
            var response = await client.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest();
            }
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
        if (section?.isDeleted == false)
        {
            return NotFound();
        }
        return Ok(section);
        // return mapper.Map<Section, TextValue>(plugin); todo
    }
}