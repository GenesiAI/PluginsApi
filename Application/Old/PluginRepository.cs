//using AiPlugin.Application.Old.Models.CompletitonPrompt;
//using AiPlugin.Application.Old.OpenAi.Models;
//using AiPlugin.Domain.Plugin;
//using AiPlugin.Infrastructure;
//using Microsoft.Extensions.Configuration;
//using System.Text;
//using System.Text.Json;

//namespace AiPlugin.Application.Old;
//public class PluginRepository : IPluginRepository
//{
//    private readonly AiPluginDbContext dbContext;
//    private readonly GPTSettings gPTSettings;
//    private HttpClient _httpClient;

//    public PluginRepository(AiPluginDbContext dbContext, GPTSettings gPTSettings, IHttpClientFactory httpClientFactory, IConfiguration configuration)
//    {
//        _httpClient = httpClientFactory.CreateClient();
//        try
//        {
//            gPTSettings = configuration.GetSection("GPTSettings").Get<GPTSettings>()!;
//        }
//        catch (Exception e)
//        {
//            Console.WriteLine("the configuration file seem to be broken" + e);
//            throw;
//        }
//        this.dbContext = dbContext;
//        this.gPTSettings = gPTSettings;
//    }
//    //old use chat endpoint
//    /// Here there are different ways to create a plugin, 
//    /// like you can pre-cut the content and let the ai describe it 
//    /// or you can let the ai split the content, the issue is tha the ai has a limited number of tokens

//    /// in order of complexity the solutions are:
//    /// 1. SNS - Split one Shot : [cheap] [small] [lower quality]
//    ///      roghtly split let the ai describe each section and the whole plugin in one shot,
//    ///      good for small plugins
//    /// 2. SDD - Split, Describe, than describe : [lower quality] [big]
//    ///      roghtly split, describe each section, on one or many shots,
//    ///      than pass the description to the ai to describe the whole plugin 
//    /// 3. ASOS - Ai Split one Shot : [cheap] [small] [higher quality]
//    ///      let the ai split the content in n sections and describe each section and 
//    ///      the whole plugin in one shot,
//    /// 4. Chunked ASAD - Chunked Ai Split and Describe, than describe AI plugin : [expensive] [big]  
//    ///      let the ai split the content and describe each section, on one or many shots,
//    ///      than pass the description to the ai to describe the whole plugin
//    ///      This could be done by sending multiple requests with the biggest chunk possible and letting the AI know
//    ///      that there is more content to come and pass to the n+1 chunk the last section of that has been provided
//    ///      as response to the n chunk with the relative description

//    ///but using chat endpoint we can use the solution 3 and 4 with a single request, maan that's cool

//    //old roadmap:
//    /// 1 - switch on model
//    ///      when more that 30K chars are provided, GPT-4 takes 8,192 tokens equal to 32768 chars,
//    /// 2 - Implement solution 3 ASOS, should be easy and fast, just create a new prompt
//    /// 3 - Implement solution 4 Chunked ASAD
//    //todo essential features:

//    /// use chat endpoint !!!!!!!
//    public async Task<Plugin> CreatePlugin(string userId, string content)
//    {

//        //split the content in chunks of 700 chars where a . 
//        //todo use ai with describe
//        var sectionsContent = SplitSection(content).ToList();

//        // var sections = await DescribeSections(sectionsContent);
//        var pluginGPT = await AskGPT(BuildPrompt(sectionsContent));

//        if (pluginGPT.Apis.Count() != sectionsContent.Count())
//            throw new Exception("the number of apis is different from the number of sections");

//        // fill a list of sections with the plugin.Apis in the same order
//        var sections = new List<Section>();
//        for (int i = 0; i < pluginGPT.Apis.Count(); i++)
//        {
//            var section = new Section
//            {
//                // PluginId = plugin.Id,
//                Name = pluginGPT.Apis[i].Name,
//                Description = pluginGPT.Apis[i].Description,
//                Content = sectionsContent[i],
//            };
//            sections.Add(section);
//        }

//        var plugin = new Plugin
//        {
//            UserId = userId,
//            //OriginalText = content,
//            Sections = sections,
//            // SchemaVersion = "1.0",
//            NameForHuman = pluginGPT.Name,
//            NameForModel = pluginGPT.Name, //generate a different name
//            DescriptionForHuman = pluginGPT.Description,
//            DescriptionForModel = pluginGPT.Description,
//            LogoUrl = "https://em-content.zobj.net/thumbs/120/microsoft/319/puzzle-piece_1f9e9.png",
//            ContactEmail = "unknown",
//            LegalInfoUrl = "unknown",
//        };
//        //save the content in the db
//        dbContext.Plugins.Add(plugin);
//        await dbContext.SaveChangesAsync();
//        return plugin;
//    }

//    private string BuildPrompt(List<string> sectionsContent)
//    {
//        if (sectionsContent == null || !sectionsContent.Any())
//            throw new ArgumentException("sectionsContent cannot be null or empty");

//        var sectionsContentJson = JsonSerializer.Serialize(sectionsContent);

//        return $$$"""
//            You have to describe an API set by its outputs, you will receive an output for each of the {{{sectionsContentJson.Count()}}} endpoints and produce a json that describe the APi and the endpoint to users and AI models.
//            Example Input: ["AiPlugin.App is a tool to create plugins from text and other sources, it is available for everyone to expand the capabilities of AIs and make everyone able to build powerful AI tools","Useful links	
//            Twitter	https://twitter.com/AIPluginApp
//            Website	https://www.aiplugin.app
//            Tool	https://www.aiplugin.app/text-to-plugin
//            Contacts	https://www.aiplugin.app/contacts
//            your plugins	https://www.aiplugin.app/yourplugins
//            ChatGPT AI	https://chat.openai.com/
//            "]
//            Expected output: {"Name": "AiPlugin.App - Build plugins from text", "Description": "A plugin to get information about a tool to build AI plugins from text data","Apis":[{"Name":"GetInformationsAboutAiTool", "Description": "Get information about AiPlugin.App a tool for everyone to expand AIs with data"},{"Name":"GetUsefulLinks","Description":"Get the list of links related to the plugin company"}]}

//            Input: {{{sectionsContentJson}}}
//            Output:
//            """;
//    }

//    // todo move in dedicated repo
//    public static class Davinci3 /*: IAIModel */
//    {
//        public const int MaxTokens = 4097;
//        public const double TokenToCharConversionRate = 3.5; //this is essential, it variates between 3 and 5
//        public const int AproximateMaxChars = (int)(MaxTokens * TokenToCharConversionRate);
//    }

//    public const int moreOrLessTheBasePromptLenght = 1256;
//    private async Task<GPTPlugin> AskGPT(string prompt)
//    {
//        //im trying to estimate if the prompt plus response (a fifth of the request) will overflow the max token
//        if (prompt.Length + prompt.Length / 5 > Davinci3.AproximateMaxChars)
//        {
//            throw new InvalidOperationException("Prompt overflow");
//        }

//        // Build the request body
//        var requestBody = new
//        {
//            model = "text-davinci-003",//prompt.Length > 2000 ? "gpt-4-32k" : "gpt-4",
//            prompt,
//            gPTSettings.temperature,
//            max_tokens = (int)((Davinci3.AproximateMaxChars - prompt.Length) / Davinci3.TokenToCharConversionRate), //gPTSettings.max_tokens, //todo gotta have something like a dictionary with model as key and maxTokens as 
//            gPTSettings.n
//        };

//        // Serialize the request body to a JSON string
//        var jsonBody = JsonSerializer.Serialize(requestBody);

//        //todo improve performances by reusing client with the factory
//        // Create a request message
//        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/completions") //todo use chat asap
//        {
//            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
//        };

//        request.Headers.Add("Authorization", "Bearer " + gPTSettings.ApiKey);

//        // Send the request and get the response
//        var response = await _httpClient.SendAsync(request);

//        // Read the response content to GPTPlugin
//        //var content = await response.Content.ReadAsStringAsync();

//        var completitionResponse = await JsonSerializer.DeserializeAsync<CompletationResponse>(response.Content.ReadAsStream());

//        return JsonSerializer.Deserialize<GPTPlugin>(completitionResponse.choices[0].text);
//    }

//    public async Task<Plugin> GetPlugin(string userId, Guid pluginId)
//    {
//        var plugin = await dbContext.Plugins.FindAsync(pluginId) ?? throw new KeyNotFoundException("Plugin not found");
//        if (plugin.UserId != userId)
//            throw new InvalidOperationException("mismatch between userId and pluginId");
//        return plugin;
//    }

//    public async Task<Section> GetSection(string userId, Guid pluginId, Guid sectionId)
//    {
//        var section = await dbContext.Sections.FindAsync(sectionId) ?? throw new KeyNotFoundException("Section not found");
//        if (section.PluginId != pluginId)
//            throw new InvalidOperationException("mismatch between pluginId and sectionId");
//        return section;
//    }

//    #region private methods

//    //temporarily code, TODO soon to use AI to describe the sections and maybe do the splitting too 
//    //private async Task<IEnumerable<Section>> DescribeSections(IEnumerable<string> sectionsContent)
//    //{
//    //    var sections = new List<Section>();
//    //    foreach (var sectionContent in sectionsContent)
//    //    {
//    //        var section = new Section
//    //        {
//    //            Name = "Section " + sections.Count,
//    //            // Description = "Description of section " + sections.Count,
//    //            Content = sectionContent
//    //        };
//    //        sections.Add(section);
//    //    }

//    //    return sections;
//    //}

//    public const int splitAt = 4000;

//    private IEnumerable<string> SplitSection(string content)
//    {
//        var sectionsContent = new List<string>();
//        var currentSectionContent = "";
//        for (int i = 0; i < content.Length; i++)
//        {
//            currentSectionContent += content[i];
//            if (i % splitAt == 0 && i > 0)
//            {
//                sectionsContent.Add(currentSectionContent);
//                currentSectionContent = "";
//            }
//        }
//        sectionsContent.Add(currentSectionContent);
//        return sectionsContent;

//    }
//    #endregion
//}
