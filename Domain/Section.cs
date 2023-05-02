namespace AiPlugin.Domain
{
    // each section has an id, name, description and a content
    public class Section
    {
        public Guid Id { get; set; }
        public Guid PluginId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}