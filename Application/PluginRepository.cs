using AiPlugin.Domain;
using AiPlugin.Infrastructure;

namespace AiPlugin.Application
{
    public class PluginRepository : IPluginRepository
    {
        private readonly AiPluginDbContext dbContext;

        public PluginRepository(AiPluginDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Plugin> CreatePlugin(Guid userId, string content)
        {

            //split the content in chunks of 700 chars where a . 
            //todo use ai with describe
            var sectionsContent = splitSection(content);

            var sections = await DescribeSections(sectionsContent);

            var plugin = new Plugin
            {
                UserId = userId,
                OriginalText = content,
                Sections = sections,
                // SchemaVersion = "1.0",
                NameForHuman = "My plugin", //todo use ai to describe the plugin
                NameForModel = "MyPlugin", //todo use ai to describe the plugin
                DescriptionForHuman = "My plugin description", //todo use ai to describe the plugin
                DescriptionForModel = "MyPluginDescription", //todo use ai to describe the plugin
                LogoUrl = "https://em-content.zobj.net/thumbs/120/microsoft/319/puzzle-piece_1f9e9.png",
                ContactEmail = "unknown",
                LegalInfoUrl = "unknown",
            };
            //save the content in the db
            dbContext.Plugins.AddAsync(plugin);
            await dbContext.SaveChangesAsync();
            return plugin;
        }


        public async Task<Plugin> GetPlugin(Guid userId, Guid pluginId)
        {
            return await dbContext.Plugins.FindAsync(pluginId) ?? throw new KeyNotFoundException("Plugin not found");
        }

        public async Task<Section> GetSection(Guid userId, Guid pluginId, Guid sectionId)
        {
            var section = await dbContext.Sections.FindAsync(sectionId) ?? throw new KeyNotFoundException("Section not found");
            if (section.PluginId != pluginId)
                throw new InvalidOperationException("mismatch between pluginId and sectionId");
            return section;
        }

        #region private methods

        //temporarily code, TODO soon to use AI to describe the sections and maybe do the splitting too 
        private async Task<IEnumerable<Section>> DescribeSections(IEnumerable<string> sectionsContent)
        {
            var sections = new List<Section>();
            foreach (var sectionContent in sectionsContent)
            {
                var section = new Section
                {
                    Name = "Section " + sections.Count,
                    // Description = "Description of section " + sections.Count,
                    Content = sectionContent
                };
                sections.Add(section);
            }

            return sections;
        }
        private IEnumerable<string> splitSection(string content)
        {
            return content.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 700)
                .Select(x => string.Join(".", x.Select(v => v.Value)))
                .ToList();
        }
    }
    #endregion
}