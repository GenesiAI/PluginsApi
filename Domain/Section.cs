namespace AiPlugin.Domain;

// each section has an id, name, description and a content
public class Section : EntityBase, IDeleted
{
    public Guid PluginId { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool isDeleted { get; set; }
}